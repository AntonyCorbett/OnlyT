using OnlyT.WebServer.Throttling;

namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Net;
    using Models;
    using Services.Options;
    using Services.TalkSchedule;
    using Services.Timer;

    internal class TimersApiController : BaseApiController
    {
        private readonly ITalkTimerService _timerService;
        private readonly ITalkScheduleService _talkScheduleService;
        private readonly IOptionsService _optionsService;

        public TimersApiController(
            ITalkTimerService timerService, 
            ITalkScheduleService talkScheduleService,
            IOptionsService optionsService)
        {
            _timerService = timerService;
            _talkScheduleService = talkScheduleService;
            _optionsService = optionsService;
        }

        public void Handler(HttpListenerRequest request, HttpListenerResponse response, ApiThrottler throttler)
        {
            CheckMethodGetOrPost(request);

            if (IsMethodGet(request))
            {
                HandleGetTimersApi(request, response, throttler);
            }
            else if (IsMethodPost(request))
            {
                HandlePostTimersApi(request, response, throttler);
            }
        }

        private void HandlePostTimersApi(
            HttpListenerRequest request, 
            HttpListenerResponse response,
            ApiThrottler throttler)
        {
            throttler.CheckRateLimit(ApiRequestType.TimerControl, request);

            throw new NotImplementedException();
        }

        private void HandleGetTimersApi(
            HttpListenerRequest request, 
            HttpListenerResponse response,
            ApiThrottler throttler)
        {
            CheckSegmentLength(request, 4, 5);

            throttler.CheckRateLimit(ApiRequestType.Timer, request);

            switch (request.Url.Segments.Length)
            {
                case 4:
                    // segments: "/" "api/" "v1/" "timers/"
                    // request for _all_ timer info...
                    WriteResponse(response, GetTimersInfo());
                    break;

                case 5:
                    // segments: "/" "api/" "v1/" "timers/" "0/"
                    // gets info for a single timer...
                    if (int.TryParse(request.Url.Segments[4], out var talkId))
                    {
                        WriteResponse(response, GetTimerInfo(talkId));
                    }

                    break;
            }
        }

        private TimersResponseData GetTimerInfo(int talkId)
        {
            return new TimersResponseData(_talkScheduleService, _timerService, _optionsService, talkId);
        }

        private TimersResponseData GetTimersInfo()
        {
            return new TimersResponseData(_talkScheduleService, _timerService, _optionsService);
        }
    }
}
