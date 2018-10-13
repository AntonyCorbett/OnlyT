namespace OnlyT.WebServer.Controllers
{
    using System.Net;

    internal class WebHooksApiController : BaseApiController
    {
        public WebHooksApiController()
        {
        }

        public void Handler(HttpListenerRequest request, HttpListenerResponse response)
        {
            CheckMethodGetPostOrDelete(request);
        }
    }
}
