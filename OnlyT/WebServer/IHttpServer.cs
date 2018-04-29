namespace OnlyT.WebServer
{
    using System;
    using EventArgs;

    public interface IHttpServer
    {
        event EventHandler<TimerInfoEventArgs> RequestForTimerDataEvent;

        void Start(int port);

        void Stop();
    }
}
