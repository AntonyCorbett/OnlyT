namespace OnlyT.WebServer.Controllers
{
    // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
    using System;
    using System.Linq;
    using System.Net;
    using System.Text;
    using ErrorHandling;
    using Newtonsoft.Json;

    internal class BaseApiController
    {
        public static void WriteResponse(HttpListenerResponse response, object info)
        {
            if (info == null)
            {
                throw new WebServerException(WebServerErrorCode.UnknownError);
            }

            try
            {
                response.ContentType = "application/json";
                response.ContentEncoding = Encoding.UTF8;

                // allow crosss-domain access (so that clients can use JS in a browser)
                response.Headers.Add("Access-Control-Allow-Origin: *");

                string jsonStr = JsonConvert.SerializeObject(info);
                byte[] buffer = Encoding.UTF8.GetBytes(jsonStr);

                response.ContentLength64 = buffer.Length;
                using (System.IO.Stream output = response.OutputStream)
                {
                    output.Write(buffer, 0, buffer.Length);
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }

        protected bool IsMethodGet(HttpListenerRequest request)
        {
            return IsMethod(request, "GET");
        }

        protected bool IsMethodPost(HttpListenerRequest request)
        {
            return IsMethod(request, "POST");
        }

        protected void CheckMethodGetOrPost(HttpListenerRequest request)
        {
            if (!IsMethodGet(request) && !IsMethodPost(request))
            {
                throw new WebServerException(WebServerErrorCode.BadHttpVerb);
            }
        }

        protected void CheckMethodGet(HttpListenerRequest request)
        {
            if (!IsMethodGet(request))
            {
                throw new WebServerException(WebServerErrorCode.BadHttpVerb);
            }
        }

        protected void CheckMethodPost(HttpListenerRequest request)
        {
            if (!IsMethodPost(request))
            {
                throw new WebServerException(WebServerErrorCode.BadHttpVerb);
            }
        }
        
        protected void CheckSegmentLength(HttpListenerRequest request, params int[] lengths)
        {
            if (!lengths.Contains(request.Url.Segments.Length))
            {
                throw new WebServerException(WebServerErrorCode.UriTooManySegments);
            }
        }

        private bool IsMethod(HttpListenerRequest request, string methodName)
        {
            return request.HttpMethod.Equals(methodName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
