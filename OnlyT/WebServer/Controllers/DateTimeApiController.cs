namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Net;
    using Models;

    internal class DateTimeApiController : BaseApiController
    {
        public void Handler(HttpListenerRequest request, HttpListenerResponse response)
        {
            CheckMethodGet(request);
            CheckSegmentLength(request, 4);

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
