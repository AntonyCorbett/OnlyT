using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.Options;
using OnlyT.WebServer.Controllers;
using Serilog;

// ReSharper disable CatchAllClause

namespace OnlyT.WebServer
{
    internal class HttpServer : IHttpServer, IDisposable
    {
        private HttpListener _listener;
        private int _port;
        private readonly bool _24HourClock;
        private readonly IOptionsService _optionsService;

        public HttpServer(IOptionsService optionsService)
        {
            _optionsService = optionsService;
            _24HourClock = new DateTime(1, 1, 1, 23, 1, 1).ToShortTimeString().Contains("23");
        }

        public void Start(int port)
        {
            if (port > 0)
            {
                _listener = new HttpListener();
                _port = port;
                Task.Factory.StartNew(StartListening);
            }
        }

        public void Stop()
        {
            if (_listener?.IsListening ?? false)
            {
                _listener?.Stop();
            }
        }

        private void StartListener()
        {
            _listener.Prefixes.Clear();
            _listener.Prefixes.Add($"http://*:{_port}/index/");
            _listener.Prefixes.Add($"http://*:{_port}/data/");
            _listener.Prefixes.Add($"http://*:{_port}/api/");

            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _listener.IgnoreWriteExceptions = true;
            _listener.Start();
        }

        private void StartListening()
        {
            try
            {
                StartListener();

                while (_listener.IsListening)
                {
                    try
                    {
                        IAsyncResult result = null;

                        if (_listener.IsListening)
                        {
                            result = _listener.BeginGetContext(ListenerCallback, _listener);
                        }

                        // Waiting for request to be processed
                        result?.AsyncWaitHandle.WaitOne();
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Could not start listening");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error during http listener initialisation");
            }
        }


        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                // Call EndGetContext to complete the asynchronous operation...
                HttpListenerContext context = _listener.EndGetContext(result);

                // Obtain a response object.
                using (HttpListenerResponse response = context.Response)
                {
                    try
                    {
                        // Construct a response. 
                        if (context.Request.Url.Segments.Length > 1)
                        {
                            // segments: "/" ...

                            string segment = context.Request.Url.Segments[1].TrimEnd('/').ToLower();
                            if (_listener.IsListening)
                            {
                                switch (segment)
                                {
                                    case "data":
                                        HandleRequestForClockWebPageTimerData(response);
                                        break;

                                    case "index":
                                        HandleRequestForClockWebPage(response);
                                        break;

                                    case "api":
                                        HandleApiRequest(context.Request, response);
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Clock server error");
                    }
                }
            }
        }

        private void HandleApiRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (_optionsService.Options.IsApiEnabled)
            {
                ApiController controller = new ApiController();
                controller.HandleRequest(request, response);
            }
        }

        private void HandleRequestForClockWebPage(HttpListenerResponse response)
        {
            if (_optionsService.Options.IsWebClockEnabled)
            {
                ClockWebPageController controller = new ClockWebPageController();
                controller.HandleRequestForWebPage(response, _24HourClock);
            }
        }

        private void HandleRequestForClockWebPageTimerData(HttpListenerResponse response)
        {
            if (_optionsService.Options.IsWebClockEnabled)
            {
                ClockWebPageController controller = new ClockWebPageController();
                controller.HandleRequestForTimerData(response);
            }
        }

        public void Dispose()
        {
            _listener.Close();
            _listener = null;
        }
    }
}
