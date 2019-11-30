namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Net;
    using Models;
    using Throttling;

    internal class DateTimeApiController : BaseApiController
    {
        private readonly ApiThrottler _apiThrottler;

        public DateTimeApiController(ApiThrottler apiThrottler)
        {
            _apiThrottler = apiThrottler;
        }

        public void Handler(
            HttpListenerRequest request, 
            HttpListenerResponse response,
            DateTime now)
        {
            CheckMethodGet(request);
            CheckSegmentLength(request, 4);

            _apiThrottler.CheckRateLimit(ApiRequestType.DateTime, request);

            // segments: "/" "api/" "v1/" "datetime/"
            // system clock
            var lt = new LocalTime
            {
                Year = now.Year,
                Month = now.Month,
                Day = now.Day,
                Hour = now.Hour,
                Min = now.Minute,
                Second = now.Second
            };

            WriteResponse(response, lt);
        }
    }
}
