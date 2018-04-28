using System;
using System.Net;
using System.Reflection;
using System.Threading;
using OnlyT.Services.Options;
using OnlyT.WebServer.ErrorHandling;
using OnlyT.WebServer.Models;

namespace OnlyT.WebServer.Controllers
{
    internal class SystemApiController : BaseApiController
    {
        private static Guid SessionId = Guid.NewGuid();
        private readonly IOptionsService _optionsService;

        public SystemApiController(IOptionsService optionsService)
        {
            _optionsService = optionsService;
        }

        public void Handler(
            HttpListenerRequest request, 
            HttpListenerResponse response, 
            int oldestSupportedApiVersion, 
            int currentApiVersion)
        {
            if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                // segments: "/" "api/" "v3/" "system/"

                if (request.Url.Segments.Length == 4)
                {
                    var systemData = GetSystemData(oldestSupportedApiVersion, currentApiVersion);
                    WriteResponse(response, systemData);
                }
                else
                {
                    throw new WebServerException(WebServerErrorCode.UriTooManySegments);
                }
            }
            else
            {
                // can only use GET on this url
                throw new WebServerException(WebServerErrorCode.BadHttpVerb);
            }
        }

        private ApiSystemData GetSystemData(int oldestSupportedApiVersion, int currentApiVersion)
        {
            var currentCulture = Thread.CurrentThread.CurrentUICulture;

            return new ApiSystemData
            {
                AccountName = Environment.UserName,
                WorkingSet = Environment.WorkingSet,
                MachineName = Environment.MachineName,
                OnlyTVersion = Assembly.GetEntryAssembly().GetName().Version.ToString(),
                SessionId = SessionId.ToString(),
                ApiEnabled = _optionsService.Options.IsApiEnabled,
                ApiCodeRequired = !string.IsNullOrEmpty(_optionsService.Options.ApiCode),
                Culture = new ApiCultureData
                {
                    Name = currentCulture.Name,
                    IsoCode2 = currentCulture.TwoLetterISOLanguageName,
                    IsoCode3 = currentCulture.ThreeLetterISOLanguageName
                },
                ApiVersion = new ApiVersion
                {
                    LowVersion = oldestSupportedApiVersion,
                    HighVersion = currentApiVersion
                }
            };
        }
    }
}
