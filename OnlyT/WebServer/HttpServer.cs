using OnlyT.Utils;

namespace OnlyT.WebServer;

using Controllers;
using ErrorHandling;
using EventArgs;
using Models;
using OnlyT.Common.Services.DateTime;
using Services.CommandLine;
using Serilog;
using Services.Bell;
using Services.Options;
using Services.TalkSchedule;
using Services.Timer;
using System;
using System.Net;
using System.Threading.Tasks;
using Throttling;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class HttpServer : IHttpServer, IDisposable
{
    private readonly IOptionsService _optionsService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ApiThrottler _apiThrottler;
    private readonly ICommandLineService _commandLineService;
    private readonly ApiRouter _apiRouter;
    private readonly ITalkTimerService _timerService;
    private readonly ITalkScheduleService _talksService;
    private HttpListener? _listener;
    private int _port;
    private readonly DedupLogger _dedupLogger = new(); // prevents excessive logging of the same error

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
        _timerService = timerService;
        _talksService = talksService;

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
            _ = Task.Run(StartListeningAsync);
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
        _listener.Prefixes.Add($"http://{ipAddress}:{_port}/schedule/");
        _listener.Prefixes.Add($"http://{ipAddress}:{_port}/api/");
    }

    private async Task StartListeningAsync()
    {
        try
        {
            StartListener();

            if (_listener == null)
            {
                Log.Logger.Warning("Could not create listener");
                return;
            }

            while (_listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleContext(context));
                }
                catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                {
                    // Listener was stopped (shutdown or reconfiguration).
                    break;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not get context");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error during http listener initialisation");
        }
    }

    private void HandleContext(HttpListenerContext context)
    {
        HttpListenerResponse? response = null;
        try
        {
            response = context.Response;

            if (context.Request.Url?.Segments.Length > 1 && (_listener?.IsListening ?? false))
            {
                var segment = context.Request.Url.Segments[1].TrimEnd('/').ToLower();

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

                    case "schedule":
                        HandleRequestForTimersScheduleData(context.Request, response);
                        break;

                    case "api":
                        HandleApiRequest(context.Request, response);
                        break;
                }
            }
        }
        catch (HttpListenerException ex) when (ex.ErrorCode == 995) { }
        catch (WebServerException ex)
        {
            _dedupLogger.LogErrorDedup(ex, "Web server error");
            WriteApiErrorResponse(response, ex.Code);
        }
        catch (Exception ex)
        {
            _dedupLogger.LogErrorDedup(ex, "Web server error");
            WriteApiErrorResponse(response, WebServerErrorCode.UnknownError);
        }
        finally
        {
            (response as IDisposable)?.Dispose();
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
        if (_optionsService.Options.IsApiEnabled || IsWebClockTimersGetRequest(request))
        {
            _apiRouter.HandleRequest(request, response);
        }
    }

    // Allows the Timers web page to fetch schedule data even when Remote Apps is disabled.
    // The timers list GET is read-only and requires the same IsWebClockEnabled flag as the page itself.
    private bool IsWebClockTimersGetRequest(HttpListenerRequest request)
    {
        if (!_optionsService.Options.IsWebClockEnabled) return false;
        if (!request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase)) return false;
        var segments = request.Url?.Segments;
        return segments?.Length >= 4
            && segments[3].TrimEnd('/').Equals("timers", StringComparison.OrdinalIgnoreCase);
    }

    private void HandleRequestForClockWebPage(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (_optionsService.Options.IsWebClockEnabled)
        {
            WebPageController.HandleRequestForWebPage(response, WebPageTypes.Clock);
        }
    }

    private void HandleRequestForTimersWebPage(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (_optionsService.Options.IsWebClockEnabled)
        {
            WebPageController.HandleRequestForWebPage(response, WebPageTypes.Timers);
        }
    }

    // Serves timer schedule data for the timers web page without throttling.
    // This path is exclusively for the page's own polling; remote apps use /api/*/timers which is throttled.
    private void HandleRequestForTimersScheduleData(HttpListenerRequest request, HttpListenerResponse response)
    {
        if (_optionsService.Options.IsWebClockEnabled)
        {
            BaseApiController.WriteResponse(response, new TimersResponseData(_talksService, _timerService, _optionsService));
        }
    }

    private void HandleRequestForClockWebPageTimerData(
        HttpListenerRequest request,
        HttpListenerResponse response)
    {
        if (_optionsService.Options.IsWebClockEnabled)
        {
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