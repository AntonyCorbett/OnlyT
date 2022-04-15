﻿using System.Net;
using OnlyT.WebServer.Models;
using OnlyT.WebServer.Throttling;
using OnlyT.Services.Bell;
using OnlyT.Services.Options;

namespace OnlyT.WebServer.Controllers
{
    internal class BellApiController : BaseApiController
    {
        private readonly IOptionsService _optionsService;
        private readonly IBellService _bellService;
        private readonly ApiThrottler _apiThrottler;

        public BellApiController(
            IOptionsService optionsService, 
            IBellService bellService,
            ApiThrottler apiThrottler)
        {
            _optionsService = optionsService;
            _bellService = bellService;
            _apiThrottler = apiThrottler;
        }

        public void Handler(HttpListenerRequest request, HttpListenerResponse response)
        {
            CheckMethodPost(request);
            CheckSegmentLength(request, 4);

            _apiThrottler.CheckRateLimit(ApiRequestType.Bell, request);

            var responseData = new BellResponseData();

            // segments: "/" "api/" "v1/" "bell/"
            if (!_bellService.IsPlaying)
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
    }
}
