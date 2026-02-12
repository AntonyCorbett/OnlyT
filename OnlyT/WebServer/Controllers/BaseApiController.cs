namespace OnlyT.WebServer.Controllers
{
    // ReSharper disable ParameterOnlyUsedForPreconditionCheck.Global
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using ErrorHandling;
    using Newtonsoft.Json;

    internal abstract class BaseApiController
    {
        protected BaseApiController()
        {
        }

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

                // allow cross-domain access (so that clients can use JS in a browser)
                response.Headers.Add("Access-Control-Allow-Origin: *");

                var jsonStr = JsonConvert.SerializeObject(info);
                var buffer = Encoding.UTF8.GetBytes(jsonStr);

                response.ContentLength64 = buffer.Length;
                using var output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        protected static bool IsMethodGet(HttpListenerRequest request)
        {
            return IsMethod(request, "GET");
        }

        protected static bool IsMethodPost(HttpListenerRequest request)
        {
            return IsMethod(request, "POST");
        }

        protected static bool IsMethodDelete(HttpListenerRequest request)
        {
            return IsMethod(request, "DELETE");
        }

        protected static void CheckMethodGetOrPost(HttpListenerRequest request)
        {
            if (!IsMethodGet(request) && !IsMethodPost(request))
            {
                throw new WebServerException(WebServerErrorCode.BadHttpVerb);
            }
        }

        protected static void CheckMethodGetPostOrDelete(HttpListenerRequest request)
        {
            if (!IsMethodGet(request) && !IsMethodPost(request) && !IsMethodDelete(request))
            {
                throw new WebServerException(WebServerErrorCode.BadHttpVerb);
            }
        }

        protected static void CheckMethodGet(HttpListenerRequest request)
        {
            if (!IsMethodGet(request))
            {
                throw new WebServerException(WebServerErrorCode.BadHttpVerb);
            }
        }

        protected static void CheckMethodPost(HttpListenerRequest request)
        {
            if (!IsMethodPost(request))
            {
                throw new WebServerException(WebServerErrorCode.BadHttpVerb);
            }
        }

        protected static T ReadRequestBody<T>(HttpListenerRequest request)
        {
            try
            {
                using var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8);
                var json = reader.ReadToEnd();

                if (string.IsNullOrWhiteSpace(json))
                {
                    throw new WebServerException(WebServerErrorCode.BadRequestBody);
                }

                var result = JsonConvert.DeserializeObject<T>(json);
                return result ?? throw new WebServerException(WebServerErrorCode.BadRequestBody);
            }
            catch (IOException)
            {
                throw new WebServerException(WebServerErrorCode.BadRequestBody);
            }
            catch (JsonException)
            {
                throw new WebServerException(WebServerErrorCode.BadRequestBody);
            }
        }
        
        protected static void CheckSegmentLength(HttpListenerRequest request, params int[] lengths)
        {
            if (!lengths.Contains(request.Url?.Segments.Length ?? int.MaxValue))
            {
                throw new WebServerException(WebServerErrorCode.UriTooManySegments);
            }
        }

        private static bool IsMethod(HttpListenerRequest request, string methodName)
        {
            return request.HttpMethod.Equals(methodName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
