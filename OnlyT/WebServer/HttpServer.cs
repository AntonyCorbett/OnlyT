namespace OnlyT.WebServer
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Controllers;
    using ErrorHandling;
    using EventArgs;
    using Models;
    using OnlyT.Common.Services.DateTime;
    using Serilog;
    using Services.Bell;
    using Services.Options;
    using Services.TalkSchedule;
    using Services.Timer;
    using Throttling;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class HttpServer : IHttpServer, IDisposable
    {
        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ApiThrottler _apiThrottler;
        private readonly ApiRouter _apiRouter;
        private HttpListener _listener;
        private int _port;

        public HttpServer(
            IOptionsService optionsService, 
            IBellService bellService,
            ITalkTimerService timerService,
            ITalkScheduleService talksService,
            IDateTimeService dateTimeService)
        {
            _optionsService = optionsService;
            _dateTimeService = dateTimeService;

            _apiThrottler = new ApiThrottler(optionsService);

            _apiRouter = new ApiRouter(
                _apiThrottler, 
                _optionsService, 
                bellService, 
                timerService, 
                talksService,
                dateTimeService);
        }

        public event EventHandler<TimerInfoEventArgs> RequestForTimerDataEvent;

        public void Dispose()
        {
            _listener.Close();
            _listener = null;
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
            _listener.Prefixes.Add($"http://*:{_port}/timers/");
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
                var context = _listener.EndGetContext(result);
                
                // Obtain a response object.
                using (var response = context.Response)
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
                                        HandleRequestForClockWebPageTimerData(context.Request, response);
                                        break;

                                    case "index":
                                        HandleRequestForClockWebPage(context.Request, response);
                                        break;

                                    case "timers":
                                        HandleRequestForTimersWebPage(context.Request, response);
                                        break;

                                    case "api":
                                        HandleApiRequest(context.Request, response);
                                        break;
                                }
                            }
                        }
                    }
                    catch (WebServerException ex)
                    {
                        Log.Logger.Error(ex, "Web server error");
                        response.StatusCode = (int)WebServerErrorCodes.GetHttpErrorCode(ex.Code);
                        BaseApiController.WriteResponse(response, new ApiError(ex.Code));
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Web server error");
                        response.StatusCode = (int)WebServerErrorCodes.GetHttpErrorCode(WebServerErrorCode.UnknownError);
                        BaseApiController.WriteResponse(response, new ApiError(WebServerErrorCode.UnknownError));
                    }
                }
            }
        }

        private void HandleApiRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (_optionsService.Options.IsApiEnabled)
            {
                _apiRouter.HandleRequest(request, response);
            }
        }

        private void HandleRequestForClockWebPage(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (_optionsService.Options.IsWebClockEnabled)
            {
                _apiThrottler.CheckRateLimit(ApiRequestType.ClockPage, request);

                WebPageController controller = new WebPageController(WebPageTypes.Clock);
                controller.HandleRequestForWebPage(response);
            }
        }

        private void HandleRequestForTimersWebPage(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (_optionsService.Options.IsWebClockEnabled)
            {
                _apiThrottler.CheckRateLimit(ApiRequestType.ClockPage, request);

                WebPageController controller = new WebPageController(WebPageTypes.Timers);
                controller.HandleRequestForWebPage(response);
            }
        }

        private void HandleRequestForClockWebPageTimerData(
            HttpListenerRequest request,
            HttpListenerResponse response)
        {
            if (_optionsService.Options.IsWebClockEnabled)
            {
                _apiThrottler.CheckRateLimit(ApiRequestType.ClockData, request);

                var timerInfo = new TimerInfoEventArgs();
                OnRequestForTimerDataEvent(timerInfo);

                WebPageController controller = new WebPageController(WebPageTypes.Clock);
                controller.HandleRequestForTimerData(response, timerInfo, _dateTimeService.Now());
            }
        }

        private void OnRequestForTimerDataEvent(TimerInfoEventArgs timerInfo)
        {
            RequestForTimerDataEvent?.Invoke(this, timerInfo);
        }
    }
}
