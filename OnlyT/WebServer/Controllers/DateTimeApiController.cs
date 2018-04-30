namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Net;
    using Models;
    using Throttling;

    internal class DateTimeApiController : BaseApiController
    {
        public void Handler(
            HttpListenerRequest request, 
            HttpListenerResponse response, 
            ApiThrottler apiThrottler)
        {
            CheckMethodGet(request);
            CheckSegmentLength(request, 4);

            apiThrottler.CheckRateLimit(ApiRequestType.DateTime, request);

            // segments: "/" "api/" "v1/" "datetime/"
            // system clock
            var dt = DateTime.Now;

            var lt = new LocalTime
            {
                Year = dt.Year,
                Month = dt.Month,
                Day = dt.Day,
                Hour = dt.Hour,
                Min = dt.Minute,
                Second = dt.Second
            };

            WriteResponse(response, lt);
        }
    }
}
