using System;

namespace OnlyT.WebServer
{
    public interface IHttpServer
    {
        void Start(int port);
        void Stop();
    }
}
