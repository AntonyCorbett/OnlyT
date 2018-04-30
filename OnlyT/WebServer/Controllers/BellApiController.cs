namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Net;
    using Models;
    using OnlyT.WebServer.Throttling;
    using Services.Bell;
    using Services.Options;

    internal class BellApiController : BaseApiController
    {
        private readonly IOptionsService _optionsService;
        private readonly IBellService _bellService;

        public BellApiController(IOptionsService optionsService, IBellService bellService)
        {
            _optionsService = optionsService;
            _bellService = bellService;
        }

        public void Handler(HttpListenerRequest request, HttpListenerResponse response)
        {
            CheckMethodPost(request);
            CheckSegmentLength(request, 4);

            var responseData = new BellResponseData();

            // segments: "/" "api/" "v1/" "bell/"
            if (_optionsService.Options.IsBellEnabled && !_bellService.IsPlaying)
            {
                responseData.Success = true;
                _bellService.Play(_optionsService.Options.BellVolumePercent);
            }
            else
            {
                responseData.Success = false;
            }

            WriteResponse(response, responseData);
        }

        internal void Handler(
            HttpListenerRequest request, 
            HttpListenerResponse response, 
            ApiThrottler throttler)
        {
            throttler.CheckRateLimit(ApiRequestType.Bell, request);

            throw new NotImplementedException();
        }
    }
}
