namespace OnlyT.WebServer.Controllers;

using System;
using System.Net;
using ErrorHandling;
using Models;
using OnlyT.Common.Services.DateTime;
using Services.Bell;
using Services.Options;
using Services.TalkSchedule;
using Services.Timer;
using Throttling;

internal sealed class ApiRouter : BaseApiController
{
    private const int OldestSupportedApiVer = 1;

    // Only increase this version number if we are making breaking changes to existing API endpoints.
    // Adding new endpoints does not require a version increase.
    private const int CurrentApiVer = 4;

    private readonly ApiThrottler _apiThrottler;
    private readonly IOptionsService _optionsService;
    private readonly IDateTimeService _dateTimeService;

    private readonly Lazy<TimersApiController> _timersApiController;
    private readonly Lazy<DateTimeApiController> _dateTimeApiController;
    private readonly Lazy<BellApiController> _bellApiController;
    private readonly Lazy<SystemApiController> _systemApiController;
        
    public ApiRouter(
        ApiThrottler apiThrottler,
        IOptionsService optionsService,
        IBellService bellService,
        ITalkTimerService timerService,
        ITalkScheduleService talksService,
        IDateTimeService dateTimeService)
    {
        _apiThrottler = apiThrottler;
        _optionsService = optionsService;
        _dateTimeService = dateTimeService;

        _timersApiController = new Lazy<TimersApiController>(() => 
            new TimersApiController(timerService, talksService, _optionsService, _apiThrottler));

        _dateTimeApiController = new Lazy<DateTimeApiController>(() =>
            new DateTimeApiController(_apiThrottler));

        _bellApiController = new Lazy<BellApiController>(() =>
            new BellApiController(_optionsService, bellService, _apiThrottler));

        _systemApiController = new Lazy<SystemApiController>(() =>
            new SystemApiController(_optionsService, _apiThrottler));
    }

    public void HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
        {
            HandleOptionsMethod(request, response);
        }
        else
        {
            if (request.Url?.Segments.Length < 2)
            {
                throw new WebServerException(WebServerErrorCode.UriTooFewSegments);
            }

            if (request.Url?.Segments.Length == 2)
            {
                // segments: "/" "api/"
                HandleApiVersionRequest(request, response);
            }
            else if (request.Url?.Segments.Length > 3)
            {
                // segments: "/" "api/" "v1"
                var apiVerStr = request.Url.Segments[2].TrimEnd('/').ToLower();
                var segment = request.Url.Segments[3].TrimEnd('/').ToLower();

                // may use this at some point.
                // Currently, the API version is validated but discarded — it's not used for routing decisions.
                // All requests to v1, v2, v3, or v4 currently behave identically. The version in the URI is
                // essentially documentation at this point.
                _ = GetApiVerFromStr(apiVerStr);

                if (!segment.Equals("system"))
                {
                    // we don't check the api code for calls to the "system" API
                    var apiCode = _optionsService.Options.ApiCode;
                    CheckApiCode(request, apiCode);
                }
                    
                switch (segment)
                {
                    case "timers":
                        _timersApiController.Value.Handler(request, response);
                        break;

                    case "webhooks":
                        WebHooksApiController.Handler(request);
                        break;

                    case "bell":
                        _bellApiController.Value.Handler(request, response);
                        break;

                    case "datetime":
                        DisableCache(response);
                        _dateTimeApiController.Value.Handler(request, response, _dateTimeService.Now());
                        break;

                    case "system":
                        // no API code check needed
                        DisableCache(response);
                        _systemApiController.Value.Handler(request, response, OldestSupportedApiVer, CurrentApiVer);
                        break;

                    default:
                        throw new WebServerException(WebServerErrorCode.BadPrefix);
                }
            }
        }
    }

    private static void DisableCache(HttpListenerResponse response)
    {
        response.AddHeader("Cache-Control", "no-cache");
    }

    private void HandleApiVersionRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        CheckMethodGet(request);

        _apiThrottler.CheckRateLimit(ApiRequestType.Version, request);

        var v = new ApiVersion { LowVersion = OldestSupportedApiVer, HighVersion = CurrentApiVer };
        WriteResponse(response, v);
    }

    private static void HandleOptionsMethod(HttpListenerRequest request, HttpListenerResponse response)
    {
        response.StatusCode = (int)HttpStatusCode.OK;

        // Always add CORS headers for OPTIONS requests, regardless of whether the client sends the Access-Control-Request-Headers:
        response.AddHeader("Access-Control-Allow-Origin", "*");
        response.AddHeader("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");

        var corsHeaders = request.Headers["Access-Control-Request-Headers"];

        if (corsHeaders != null)
        {
            response.AddHeader("Access-Control-Allow-Headers", corsHeaders);
        }
        else
        {
            // Default headers if not specified
            response.AddHeader("Access-Control-Allow-Headers", "Content-Type, x-api-code");
        }
    }

    private static int GetApiVerFromStr(string apiVerStr)
    {
        if (!int.TryParse(apiVerStr[1..], out var ver) || ver < 1 || ver > CurrentApiVer)
        {
            throw new WebServerException(WebServerErrorCode.ApiVersionNotSupported);
        }

        return ver;
    }

    private static void CheckApiCode(HttpListenerRequest request, string? apiCode)
    {
        // use of api code is enabled... skip check if it's a GET request
        var code = request.Headers["ApiCode"] ?? request.QueryString["ApiCode"];
        if (string.IsNullOrEmpty(code) && (string.IsNullOrEmpty(apiCode) || request.HttpMethod.Equals("GET")))
        {
            return;
        }

        if (code != null && code.Equals(apiCode))
        {
            return;
        }

        throw new WebServerException(WebServerErrorCode.BadApiCode);
    }
}