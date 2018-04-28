using System;
using System.Net;
using OnlyT.Services.Options;
using OnlyT.WebServer.ErrorHandling;
using OnlyT.WebServer.Models;

namespace OnlyT.WebServer.Controllers
{
    internal class ApiRouter : BaseApiController
    {
        private const int OldestSupportedApiVer = 1;
        private const int CurrentApiVer = 1;

        public void HandleRequest(
            HttpListenerRequest request, 
            HttpListenerResponse response, 
            IOptionsService optionsService)
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
                    // segments: "/" "api/" "v1" ...

                    string apiVerStr = request.Url.Segments[2].TrimEnd('/').ToLower();
                    string segment = request.Url.Segments[3].TrimEnd('/').ToLower();

                    int apiVer = GetApiVerFromStr(apiVerStr);
                    
                    switch (segment)
                    {
                        //case "timers":
                        //    CheckApiCode(request, apiCode);
                        //    HandleTimersApi(apiVer, request, response);
                        //    break;

                        //case "events":
                        //    if (_tcpNotifier != null)
                        //    {
                        //        CheckApiCode(request, apiCode);
                        //        HandleEventsApi(apiVer, request, response);
                        //    }
                        //    break;

                        //case "bell":
                        //    CheckApiCode(request, apiCode);
                        //    HandleBellApi(apiVer, request, response);
                        //    break;

                        //case "datetime":
                        //    CheckApiCode(request, apiCode);
                        //    DisableCache(response);
                        //    HandleTimeApi(apiVer, request, response);
                        //    break;

                        case "system":
                            // no API code check needed
                            DisableCache(response);
                            HandleSystemApi(apiVer, request, response, optionsService);
                            break;

                        default:
                            throw new WebServerException(WebServerErrorCode.BadPrefix);
                    }
                }
            }
        }

        private void HandleSystemApi(
            int apiVer, 
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

            string corsHeaders = request.Headers["Access-Control-Request-Headers"];

            if (corsHeaders != null)
            {
                response.AddHeader("Access-Control-Allow-Headers", corsHeaders);
                response.AddHeader("Access-Control-Allow-Methods", "GET, PUT, POST, DELETE, OPTIONS");
                response.AddHeader("Access-Control-Allow-Origin", "*");
            }
        }

        private int GetApiVerFromStr(string apiVerStr)
        {
            if (!Int32.TryParse(apiVerStr.Substring(1), out var ver) || ver < 1 || ver > CurrentApiVer)
            {
                throw new WebServerException(WebServerErrorCode.ApiVersionNotSupported);
            }
            return ver;
        }

        private void CheckApiCode(HttpListenerRequest request, string apiCode)
        {
            // use of api code is enabled...
            var code = request.Headers["ApiCode"];
            if (!code.Equals(apiCode))
            {
                var qs = request.QueryString["ApiCode"];
                if (qs == null || !qs.Equals(apiCode))
                {
                    throw new WebServerException(WebServerErrorCode.BadApiCode);
                }
            }
        }
    }
}
