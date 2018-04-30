using System.Diagnostics;
using OnlyT.Services.Options;

namespace OnlyT.WebServer.Throttling
{
    using System.Net;

    internal class ApiThrottler
    {
        private readonly ApiRequestHistory _requestHistory;
        private readonly Stopwatch _stopwatch;
        private readonly IOptionsService _optionsService;

        public ApiThrottler(IOptionsService optionsService)
        {
            _optionsService = optionsService;
            _requestHistory = new ApiRequestHistory();
            _stopwatch = Stopwatch.StartNew();
        }

        public void CheckRateLimit(ApiRequestType requestType, HttpListenerRequest request)
        {
            if (_optionsService.Options.IsApiThrottled)
            {
                _requestHistory.Add(GetClientId(request), requestType, _stopwatch.ElapsedMilliseconds);
            }
        }

        private string GetClientId(HttpListenerRequest request)
        {
            return request.RemoteEndPoint?.ToString();
        }
    }
}
