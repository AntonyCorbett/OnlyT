using System;

namespace OnlyT.WebServer.Controllers;

using System.Net;
using ErrorHandling;
using Models;
using Services.Options;
using Services.TalkSchedule;
using Services.Timer;
using Throttling;

internal sealed class TimersApiController : BaseApiController
{
    private readonly ITalkTimerService _timerService;
    private readonly ITalkScheduleService _talkScheduleService;
    private readonly IOptionsService _optionsService;
    private readonly ApiThrottler _apiThrottler;

    public TimersApiController(
        ITalkTimerService timerService, 
        ITalkScheduleService talkScheduleService,
        IOptionsService optionsService,
        ApiThrottler apiThrottler)
    {
        _timerService = timerService;
        _talkScheduleService = talkScheduleService;
        _optionsService = optionsService;
        _apiThrottler = apiThrottler;
    }

    public void Handler(HttpListenerRequest request, HttpListenerResponse response)
    {
        // duration sub-path: /api/v4/timers/{talkId}/duration
        if (request.Url?.Segments.Length == 6 &&
            request.Url.Segments[5].TrimEnd('/').Equals("duration", StringComparison.OrdinalIgnoreCase))
        {
            CheckMethodPost(request);
            HandlePostTimerDurationApi(request, response);
            return;
        }

        CheckMethodGetPostOrDelete(request);

        if (IsMethodGet(request))
        {
            // get timer info
            HandleGetTimersApi(request, response);
        }
        else if (IsMethodPost(request))
        {
            // start a timer
            HandlePostTimersApi(request, response);
        }
        else if (IsMethodDelete(request))
        {
            // stop a timer
            HandleDeleteTimersApi(request, response);
        }
    }

    private void HandleDeleteTimersApi(HttpListenerRequest request, HttpListenerResponse response)
    {
        CheckSegmentLength(request, 5);

        _apiThrottler.CheckRateLimit(ApiRequestType.TimerControlStop, request);

        if (int.TryParse(request.Url?.Segments[4], out var talkId))
        {
            WriteResponse(response, _timerService.StopTalkTimerFromApi(talkId));
        }
    }

    private void HandlePostTimersApi(HttpListenerRequest request, HttpListenerResponse response)
    {
        CheckSegmentLength(request, 5);

        _apiThrottler.CheckRateLimit(ApiRequestType.TimerControlStart, request);

        if (int.TryParse(request.Url?.Segments[4], out var talkId))
        {
            WriteResponse(response, _timerService.StartTalkTimerFromApi(talkId));
        }
    }

    private void HandleGetTimersApi(HttpListenerRequest request, HttpListenerResponse response)
    {
        CheckSegmentLength(request, 4, 5);

        _apiThrottler.CheckRateLimit(ApiRequestType.Timer, request);

        switch (request.Url?.Segments.Length)
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

            default:
                throw new NotSupportedException();
        }
    }

    private TimersResponseData GetTimerInfo(int talkId)
    {
        return new TimersResponseData(_talkScheduleService, _timerService, _optionsService, talkId);
    }

    private void HandlePostTimerDurationApi(HttpListenerRequest request, HttpListenerResponse response)
    {
        _apiThrottler.CheckRateLimit(ApiRequestType.TimerDurationChange, request);

        if (int.TryParse(request.Url?.Segments[4].TrimEnd('/'), out var talkId))
        {
            var body = ReadRequestBody<TimerDurationChangeRequest>(request);

            if (body.DeltaSeconds == null)
            {
                throw new WebServerException(WebServerErrorCode.BadRequestBody);
            }

            var result = _timerService.ChangeDurationFromApi(talkId, body.DeltaSeconds.Value);
            WriteResponse(response, result);
        }
    }

    private TimersResponseData GetTimersInfo()
    {
        return new TimersResponseData(_talkScheduleService, _timerService, _optionsService);
    }
}