using System;
using System.Net;
using OnlyT.WebServer.Models;

namespace OnlyT.WebServer.Controllers
{
    internal class DateTimeApiController : BaseApiController
    {
        public void Handler(HttpListenerRequest request, HttpListenerResponse response)
        {
            CheckMethodGet(request);
            CheckSegmentLength(request, 4);

            // segments: "/" "api/" "v1/" "datetime/"

            // system clock
            DateTime dt = DateTime.Now;

            LocalTime lt = new LocalTime
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
