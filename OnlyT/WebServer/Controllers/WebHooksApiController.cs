﻿namespace OnlyT.WebServer.Controllers
{
    using System.Net;

    internal class WebHooksApiController : BaseApiController
    {
        public static void Handler(HttpListenerRequest request)
        {
            CheckMethodGetPostOrDelete(request);
        }
    }
}
