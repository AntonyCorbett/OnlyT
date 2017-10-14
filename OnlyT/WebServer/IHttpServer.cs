using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.WebServer
{
    public interface IHttpServer
    {
        event EventHandler<ClockServerEventArgs> ClockServerRequestHandler;
        void Start(int port);
        void Stop();
    }
}
