using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.WebServer.Controllers
{
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
