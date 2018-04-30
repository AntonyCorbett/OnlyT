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

            ApiClientIdAndRequestType key = new ApiClientIdAndRequestType(clientId, requestType);
            var found = _clientHistory.TryGetValue(key, out var stamp);
            _clientHistory.AddOrUpdate(key, currentStamp, (keyType, stampType) => currentStamp);

            if (found)
            {
                long minIntervalMillisecs = GetMinCallingInterval(requestType);

                if (currentStamp - stamp < minIntervalMillisecs)
                {
                    throw new WebServerException(WebServerErrorCode.Throttled);
                }
            }
        }

        private long GetMinCallingInterval(ApiRequestType requestType)
        {
            switch (requestType)
            {
                case ApiRequestType.Timer:
                    return 2000;

                case ApiRequestType.TimerControl:
                    return 500;
                    
                default:
                case ApiRequestType.Version:
                case ApiRequestType.ClockData:
                case ApiRequestType.ClockPage:
                case ApiRequestType.Bell:
                case ApiRequestType.DateTime:
                case ApiRequestType.System:
                    return 1000;
            }
        }
    }
}
