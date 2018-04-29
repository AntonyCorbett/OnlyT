namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Net;
    using ErrorHandling;
    using Models;
    using Services.Bell;
    using Services.Options;
    using Services.TalkSchedule;
    using Services.Timer;

    internal class ApiRouter : BaseApiController
    {
        private const int OldestSupportedApiVer = 1;
        private const int CurrentApiVer = 1;

        public void HandleRequest(
            HttpListenerRequest request, 
            HttpListenerResponse response, 
            IOptionsService optionsService,
            IBellService bellService,
            ITalkTimerService timerService,
            ITalkScheduleService talksService)
        {
            if (request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                HandleOptionsMethod(request, response);
            }
            else
            {
                var apiCode = optionsService.Options.ApiCode;

                if (request.Url.Segments.Length < 2)
                {
                    throw new WebServerException(WebServerErrorCode.UriTooFewSegments);
                }

                if (request.Url.Segments.Length == 2)
                {
                    // segments: "/" "api/"
                    HandleApiVersionRequest(response);
                }
                else if (request.Url.Segments.Length > 3)
                {
                    // segments: "/" "api/" "v1"
                    string apiVerStr = request.Url.Segments[2].TrimEnd('/').ToLower();
                    string segment = request.Url.Segments[3].TrimEnd('/').ToLower();

                    int apiVer = GetApiVerFromStr(apiVerStr);

                    if (!segment.Equals("system"))
                    {
                        // we don't check the api code for calls to the "system" API
                        CheckApiCode(request, apiCode);
                    }
                    
                    switch (segment)
                    {
                        case "timers":
                            HandleTimersApi(request, response, timerService, talksService, optionsService);
                            break;

                        //case "events":
                        //    if (_tcpNotifier != null)
                        //    {
                        //        HandleEventsApi(apiVer, request, response);
                        //    }
                        //    break;

                        case "bell":
                            HandleBellApi(request, response, optionsService, bellService);
                            break;

                        case "datetime":
                            DisableCache(response);
                            HandleDateTimeApi(request, response);
                            break;

                        case "system":
                            // no API code check needed
                            DisableCache(response);
                            HandleSystemApi(request, response, optionsService);
                            break;

                        default:
                            throw new WebServerException(WebServerErrorCode.BadPrefix);
                    }
                }
            }
        }

        private void HandleTimersApi(
            HttpListenerRequest request, 
            HttpListenerResponse response,
            ITalkTimerService timerService,
            ITalkScheduleService talksService,
            IOptionsService optionsService)
        {
            var controller = new TimersApiController(timerService, talksService, optionsService);
            controller.Handler(request, response);
        }

        private void HandleBellApi(
            HttpListenerRequest request, 
            HttpListenerResponse response,
            IOptionsService optionsService,
            IBellService bellService)
        {
            var controller = new BellApiController(optionsService, bellService);
            controller.Handler(request, response);
        }

        private void HandleDateTimeApi(HttpListenerRequest request, HttpListenerResponse response)
        {
            var controller = new DateTimeApiController();
            controller.Handler(request, response);
        }

        private void HandleSystemApi(
            HttpListenerRequest request, 
            HttpListenerResponse response, 
            IOptionsService optionsService)
        {
            var controller = new SystemApiController(optionsService);
            controller.Handler(request, response, OldestSupportedApiVer, CurrentApiVer);
        }

        private void DisableCache(HttpListenerResponse response)
        {
            response.AddHeader("Cache-Control", "no-cache");
        }

        private void HandleApiVersionRequest(HttpListenerResponse response)
        {
            var v = new ApiVersion { LowVersion = OldestSupportedApiVer, HighVersion = CurrentApiVer };
            WriteResponse(response, v);
        }

        private void HandleOptionsMethod(HttpListenerRequest request, HttpListenerResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.OK;

            var corsHeaders = request.Headers["Access-Control-Request-Headers"];

            if (corsHeaders != null)
            {
                response.AddHeader("Access-Control-Allow-Headers", corsHeaders);
                response.AddHeader("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Origin", "*");
            }
        }

        private int GetApiVerFromStr(string apiVerStr)
        {
            if (!int.TryParse(apiVerStr.Substring(1), out var ver) || ver < 1 || ver > CurrentApiVer)
            {
                throw new WebServerException(WebServerErrorCode.ApiVersionNotSupported);
            }

            return ver;
        }

        private void CheckApiCode(HttpListenerRequest request, string apiCode)
        {
            // use of api code is enabled...
            var code = request.Headers["ApiCode"] ?? request.QueryString["ApiCode"];
            if (string.IsNullOrEmpty(code) && string.IsNullOrEmpty(apiCode))
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
}
