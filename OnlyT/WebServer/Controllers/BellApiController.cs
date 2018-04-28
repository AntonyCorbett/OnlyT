using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.Bell;
using OnlyT.Services.Options;
using OnlyT.WebServer.Models;

namespace OnlyT.WebServer.Controllers
{
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
    }
}
