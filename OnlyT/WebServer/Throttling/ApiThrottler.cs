namespace OnlyT.WebServer.Throttling;

using Services.Options;
using System.Diagnostics;
using System.Net;
using System.Threading;

internal sealed class ApiThrottler
{
    private readonly ApiRequestHistory _requestHistory;
    private readonly Stopwatch _stopwatch;
    private readonly IOptionsService _optionsService;
    private long _lastCleanupMillisecs;

    public ApiThrottler(IOptionsService optionsService)
    {
        _optionsService = optionsService;
        _requestHistory = new ApiRequestHistory();
        _stopwatch = Stopwatch.StartNew();
        _lastCleanupMillisecs = 0;
    }

    public void CheckRateLimit(ApiRequestType requestType, HttpListenerRequest request)
    {
        if (_optionsService.Options.IsApiThrottled)
        {
            var currentMillisecs = _stopwatch.ElapsedMilliseconds;
            _requestHistory.Add(GetClientId(request) ?? "unknown client", requestType, currentMillisecs);

            // Cleanup every 5 minutes - only one thread will succeed
            var lastCleanup = Interlocked.Read(ref _lastCleanupMillisecs);
            if (currentMillisecs - lastCleanup > 300000)
            {
                // Try to claim cleanup responsibility atomically
                if (Interlocked.CompareExchange(ref _lastCleanupMillisecs, currentMillisecs, lastCleanup) == lastCleanup)
                {
                    _requestHistory.Cleanup(currentMillisecs);
                }
            }
        }
    }

    private static string? GetClientId(HttpListenerRequest request)
    {
        return request.RemoteEndPoint?.ToString();
    }
}