using System;

namespace OnlyT.WebServer
{
    public interface IHttpServer
    {
        event EventHandler<ClockServerEventArgs> ClockServerRequestHandler;
        void Start(int port);
        void Stop();
    }
}
