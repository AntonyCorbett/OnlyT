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
    using OnlyT.Services.CommandLine;
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
        private readonly ICommandLineService _commandLineService;
        private readonly ApiRouter _apiRouter;
        private HttpListener? _listener;
        private int _port;

        public HttpServer(
            IOptionsService optionsService, 
            IBellService bellService,
            ITalkTimerService timerService,
            ICommandLineService commandLineService,
            ITalkScheduleService talksService,
            IDateTimeService dateTimeService)
        {
            _optionsService = optionsService;
            _dateTimeService = dateTimeService;
            _commandLineService = commandLineService;

            _apiThrottler = new ApiThrottler(optionsService);

            _apiRouter = new ApiRouter(
                _apiThrottler, 
                _optionsService, 
                bellService, 
                timerService, 
                talksService,
                dateTimeService);
        }

        public event EventHandler<TimerInfoEventArgs>? RequestForTimerDataEvent;

        public void Dispose()
        {
            _listener?.Close();
            _listener = null;
        }

        public void Start(int port)
        {
            if (port > 0)
            {
                _listener = new HttpListener();
               
                _port = port;
                Task.Factory.StartNew((_) => StartListening(), TaskCreationOptions.LongRunning);
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
            if (_listener == null)
            {
                throw new Exception("Listener is null");
            }

            SetPrefixes();

            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _listener.IgnoreWriteExceptions = true;
            _listener.Start();
        }

        private void SetPrefixes()
        {
            if (_listener == null)
            {
                throw new Exception("Listener is null");
            }

            _listener.Prefixes.Clear();

            var ipAddress = _commandLineService.RemoteIpAddress;
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = "*"; // Listen on all IP addresses
            }

            _listener.Prefixes.Add($"http://{ipAddress}:{_port}/index/");
            _listener.Prefixes.Add($"http://{ipAddress}:{_port}/timers/");
            _listener.Prefixes.Add($"http://{ipAddress}:{_port}/data/");
            _listener.Prefixes.Add($"http://{ipAddress}:{_port}/api/");
        }

        private void StartListening()
        {
            try
            {
                StartListener();

                if(_listener == null)
                {
                    Log.Logger.Warning("Could not create listener");
                    return;
                }

                while (_listener.IsListening)
                {
                    try
                    {
                        IAsyncResult? result = null;

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
            if (_listener?.IsListening ?? false)
            {
                HttpListenerResponse? response = null;
                try
                {
                    // Call EndGetContext to complete the asynchronous operation...
                    var context = _listener.EndGetContext(result);

                    // Obtain a response object.
                    using (response = context.Response)
                    {
                        // Construct a response. 
                        if (context.Request.Url?.Segments.Length > 1)
                        {
                            // segments: "/" ...
                            var segment = context.Request.Url.Segments[1].TrimEnd('/').ToLower();
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

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                {
                    // Listener was stopped, e.g. during shutdown or
                    // when manually reconfiguring the listener.
                    // Ignore this exception.
                }
                catch (WebServerException ex)
                {
                    Log.Logger.Error(ex, "Web server error");
                    WriteApiErrorResponse(response, ex.Code);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Web server error");
                    WriteApiErrorResponse(response, WebServerErrorCode.UnknownError);                   
                }
            }
        }

        private static void WriteApiErrorResponse(HttpListenerResponse? response, WebServerErrorCode code)
        {
            if (response == null)
            {
                return;
            }

            try
            {
                response.StatusCode = (int)WebServerErrorCodes.GetHttpErrorCode(code);
                BaseApiController.WriteResponse(response, new ApiError(code));
            }
            catch (ObjectDisposedException)
            {
                // ignore as this is expected when we meet some API errors.
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

                var controller = new WebPageController(WebPageTypes.Clock);
                controller.HandleRequestForWebPage(response);
            }
        }

        private void HandleRequestForTimersWebPage(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (_optionsService.Options.IsWebClockEnabled)
            {
                _apiThrottler.CheckRateLimit(ApiRequestType.ClockPage, request);

                var controller = new WebPageController(WebPageTypes.Timers);
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
                
                WebPageController.HandleRequestForTimerData(response, timerInfo, _dateTimeService.Now());
            }
        }

        private void OnRequestForTimerDataEvent(TimerInfoEventArgs timerInfo)
        {
            RequestForTimerDataEvent?.Invoke(this, timerInfo);
        }
    }
}
