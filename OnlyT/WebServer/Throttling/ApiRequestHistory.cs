// ReSharper disable RedundantSwitchExpressionArms

namespace OnlyT.WebServer.Throttling;

using ErrorHandling;
using System.Collections.Concurrent;
using System.Linq;

internal sealed class ApiRequestHistory
{
    private readonly ConcurrentDictionary<ApiClientIdAndRequestType, long> _clientHistory;

    // ReSharper disable once ConvertConstructorToMemberInitializers
    public ApiRequestHistory()
    {
        _clientHistory = new ConcurrentDictionary<ApiClientIdAndRequestType, long>();
    }

    public void Add(string clientId, ApiRequestType requestType, long currentStamp)
    {
        if (string.IsNullOrEmpty(clientId))
        {
            return;
        }

        var key = new ApiClientIdAndRequestType(clientId, requestType);
        var minIntervalMillisecs = GetMinCallingInterval(requestType);

        _clientHistory.AddOrUpdate(
            key,
            // Add factory - first request from this client/endpoint
            addValueFactory: _ => currentStamp,
            // Update factory - subsequent request
            updateValueFactory: (_, previousStamp) =>
            {
                if (currentStamp - previousStamp < minIntervalMillisecs)
                {
                    throw new WebServerException(WebServerErrorCode.Throttled);
                }
                return currentStamp;
            });
    }

    public void Cleanup(long currentStamp, long maxAgeMillisecs = 300000) // 5 minutes
    {
        var threshold = currentStamp - maxAgeMillisecs;
        var keysToRemove = _clientHistory
            .Where(kvp => kvp.Value < threshold)
            .Select(kvp => kvp.Key).ToArray();

#pragma warning disable U2U1203
        foreach (var key in keysToRemove)
#pragma warning restore U2U1203
        {
            _clientHistory.TryRemove(key, out _);
        }
    }

    private static long GetMinCallingInterval(ApiRequestType requestType)
    {
        return requestType switch
        {
            ApiRequestType.Timer => 2000,
            ApiRequestType.TimerControlStart => 500,
            ApiRequestType.TimerControlStop => 500,
            ApiRequestType.ClockData => 500,
            ApiRequestType.Version => 1000,
            ApiRequestType.ClockPage => 1000,
            ApiRequestType.Bell => 1000,
            ApiRequestType.DateTime => 1000,
            ApiRequestType.System => 1000,
            _ => 1000
        };
    }
}