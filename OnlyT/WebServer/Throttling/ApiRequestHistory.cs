namespace OnlyT.WebServer.Throttling
{
    using System.Collections.Concurrent;
    using ErrorHandling;

    internal class ApiRequestHistory
    {
        private readonly ConcurrentDictionary<ApiClientIdAndRequestType, long> _clientHistory;

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
            var found = _clientHistory.TryGetValue(key, out var stamp);
            _clientHistory.AddOrUpdate(key, currentStamp, (_, _) => currentStamp);

            if (found)
            {
                var minIntervalMillisecs = GetMinCallingInterval(requestType);

                if (currentStamp - stamp < minIntervalMillisecs)
                {
                    throw new WebServerException(WebServerErrorCode.Throttled);
                }
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
}
