using System;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using OnlyT.WebServer.ErrorHandling;

namespace OnlyT.WebServer.Controllers
{
    internal class BaseApiController
    {
        protected void WriteResponse(HttpListenerResponse response, object info)
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
                ; // ignore
            }
        }
    }
}
