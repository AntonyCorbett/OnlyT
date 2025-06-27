// ReSharper disable CatchAllClause

// Ignore Spelling: Snackbar

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OnlyT.EventArgs;
using MaterialDesignThemes.Wpf;
using OnlyT.ViewModel.Messages;
using OnlyT.Common.Services.DateTime;
using OnlyT.Models;
using OnlyT.Services.OutputDisplays;
using OnlyT.Services.Snackbar;
using Serilog;
using OnlyT.Services.CommandLine;
using OnlyT.Services.CountdownTimer;
using OnlyT.Services.Options;
using OnlyT.Services.Timer;
using OnlyT.WebServer;
using OnlyT.Windows;
using OnlyT.EventTracking;
using OnlyT.Services.OverrunNotificationService;
using OnlyT.Services.Reminders;

namespace OnlyT.ViewModel;

/// <inheritdoc />
/// <summary>
/// View model for the main page (which is a placeholder for the Operator or Settings page)
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class MainViewModel : ObservableObject
{
    private readonly Dictionary<string, FrameworkElement> _pages = [];
    private readonly IReminderService _reminderService;
    private readonly IOverrunService _overrunService;
    private readonly IOptionsService _optionsService;
    private readonly ICountdownTimerTriggerService _countdownTimerTriggerService;
    private readonly ITalkTimerService _timerService;
    private readonly ICommandLineService _commandLineService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ITimerOutputDisplayService _timerOutputDisplayService;
    private readonly ICountdownOutputDisplayService _countdownDisplayService;
    private readonly IHttpServer _httpServer;
    private readonly ISnackbarService _snackbarService;
    private DispatcherTimer _heartbeatTimer = null!;
    private FrameworkElement? _currentPage;
    private DateTime _lastRefreshedSchedule = DateTime.MinValue;

    public MainViewModel(
        IReminderService reminderService,
        IOverrunService overrunService,
        IOptionsService optionsService,
        ITalkTimerService timerService,
        ISnackbarService snackbarService,
        IHttpServer httpServer,
        ICommandLineService commandLineService,
        ICountdownTimerTriggerService countdownTimerTriggerService,
        IDateTimeService dateTimeService,
        ITimerOutputDisplayService timerOutputDisplayService,
        ICountdownOutputDisplayService countdownDisplayService)
    {
        _reminderService = reminderService;
        _overrunService = overrunService;
        _commandLineService = commandLineService;
        _dateTimeService = dateTimeService;
        _timerOutputDisplayService = timerOutputDisplayService;
        _countdownDisplayService = countdownDisplayService;

        if (commandLineService.NoGpu || ForceSoftwareRendering())
        {
            // disable hardware (GPU) rendering so that it's all done by the CPU...
            EventTracker.Track(EventName.DisableGPU);
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
        }

        _snackbarService = snackbarService;
        _optionsService = optionsService;
        _httpServer = httpServer;
        _timerService = timerService;
        _countdownTimerTriggerService = countdownTimerTriggerService;

        _httpServer.RequestForTimerDataEvent += OnRequestForTimerData;

        // subscriptions...
        WeakReferenceMessenger.Default.Register<NavigateMessage>(this, OnNavigate);
        WeakReferenceMessenger.Default.Register<TimerMonitorChangedMessage>(this, OnTimerMonitorChanged);
        WeakReferenceMessenger.Default.Register<CountdownMonitorChangedMessage>(this, OnCountdownMonitorChanged);
        WeakReferenceMessenger.Default.Register<AlwaysOnTopChangedMessage>(this, OnAlwaysOnTopChanged);
        WeakReferenceMessenger.Default.Register<HttpServerChangedMessage>(this, OnHttpServerChanged);
        WeakReferenceMessenger.Default.Register<StopCountDownMessage>(this, OnStopCountdown);

        InitHttpServer();

        // should really create a "page service" rather than create views in the main view model!
        _pages.Add(OperatorPageViewModel.PageName, new OperatorPage());

        WeakReferenceMessenger.Default.Send(new BeforeNavigateMessage(null, OperatorPageViewModel.PageName, null));
        WeakReferenceMessenger.Default.Send(new NavigateMessage(null, OperatorPageViewModel.PageName, null));

        // (fire and forget)
        Task.Run(LaunchTimerWindowAsync);
        
        InitHeartbeatTimer();
    }

    public ISnackbarMessageQueue TheSnackbarMessageQueue => _snackbarService.TheSnackbarMessageQueue;

    public FrameworkElement? CurrentPage
    {
        get => _currentPage;
        set
        {
            if (!ReferenceEquals(_currentPage, value))
            {
                _currentPage = value;
                OnPropertyChanged();
            }
        }
    }

    public bool AlwaysOnTop =>
        _optionsService.Options.AlwaysOnTop ||
        _timerOutputDisplayService.IsWindowVisible() ||
        _countdownDisplayService.IsWindowVisible();

    public string? CurrentPageName { get; private set; }

    private bool CountDownActive => _countdownDisplayService.IsCountingDown;

    public void Closing(CancelEventArgs e)
    {
        e.Cancel = _timerService.IsRunning;
        if (!e.Cancel)
        {
            WeakReferenceMessenger.Default.Send(new ShutDownMessage(CurrentPageName));
            CloseTimerWindow();
            CloseCountdownWindow();

            _reminderService.Shutdown();
            _overrunService.Shutdown();
        }
    }

    private void CloseCountdownWindow()
    {
        try
        {
            _countdownDisplayService.Close();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Could not close countdown window");
        }
    }

    private void InitSettingsPage()
    {
        // we only init the settings page when first used.
        if (!_pages.ContainsKey(SettingsPageViewModel.PageName))
        {
            _pages.Add(SettingsPageViewModel.PageName, new SettingsPage(_commandLineService));
        }
    }

    private void OnHttpServerChanged(object recipient, HttpServerChangedMessage msg)
    {
        try
        {
            _httpServer.Stop();
            InitHttpServer();
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not reinitialise http listener";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    private void InitHttpServer()
    {
        if (_optionsService.Options.IsWebClockEnabled || _optionsService.Options.IsApiEnabled)
        {
            EventTracker.Track(EventName.InitHttpServer);
            _httpServer.Start(_optionsService.Options.HttpServerPort);
        }
    }

    private void OnRequestForTimerData(object? sender, TimerInfoEventArgs timerData)
    {
        // we received a web request for the timer clock info...
        var info = _timerService.GetClockRequestInfo();

        timerData.Use24HrFormat = _optionsService.Use24HrClockFormat();

        if (!info.IsRunning)
        {
            timerData.Mode = ClockServerMode.TimeOfDay;
        }
        else
        {
            timerData.Mode = ClockServerMode.Timer;

            timerData.TargetSecs = info.TargetSeconds;
            timerData.Mins = (int)info.ElapsedTime.TotalMinutes;
            timerData.Secs = info.ElapsedTime.Seconds;
            timerData.Millisecs = info.ElapsedTime.Milliseconds;
            timerData.ClosingSecs = info.ClosingSecs;

            timerData.IsCountingUp = info.IsCountingUp;
        }
    }

    /// <summary>
    /// Responds to change in the application's "Always on top" option.
    /// </summary>
    /// <param name="message">AlwaysOnTopChangedMessage message.</param>
    private void OnAlwaysOnTopChanged(object recipient, AlwaysOnTopChangedMessage message)
    {
        OnPropertyChanged(nameof(AlwaysOnTop));
    }

    /// <summary>
    /// Responds to a change in timer monitor.
    /// </summary>
    /// <param name="message">TimerMonitorChangedMessage message.</param>
    private void OnTimerMonitorChanged(object recipient, TimerMonitorChangedMessage message)
    {
        try
        {
            if (message.Change == MonitorChangeDescription.WindowToNone ||
                message.Change == MonitorChangeDescription.WindowToMonitor)
            {
                _timerOutputDisplayService.SaveWindowedPos();
            }

            switch (message.Change)
            {
                case MonitorChangeDescription.MonitorToMonitor:
                    RelocateTimerWindow();
                    break;

                case MonitorChangeDescription.WindowToMonitor:
                case MonitorChangeDescription.NoneToMonitor:
                    _timerOutputDisplayService.OpenWindowInMonitor();
                    break;

                case MonitorChangeDescription.MonitorToWindow:
                case MonitorChangeDescription.NoneToWindow:
                    _timerOutputDisplayService.OpenWindowWindowed();
                    break;

                case MonitorChangeDescription.WindowToNone:
                case MonitorChangeDescription.MonitorToNone:
                    _timerOutputDisplayService.HideWindow();
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (CountDownActive)
            {
                // ensure countdown remains topmost if running
                _countdownDisplayService.Activate();
            }

            OnPropertyChanged(nameof(AlwaysOnTop));
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not change monitor";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    /// <summary>
    /// Responds to a change in countdown monitor.
    /// </summary>
    /// <param name="message">CountdownMonitorChangedMessage message.</param>
    private void OnCountdownMonitorChanged(object recipient, CountdownMonitorChangedMessage message)
    {
        try
        {
            if (message.Change == MonitorChangeDescription.WindowToNone ||
                message.Change == MonitorChangeDescription.WindowToMonitor)
            {
                _countdownDisplayService.SaveWindowedPos();
            }

            switch (message.Change)
            {
                case MonitorChangeDescription.MonitorToMonitor:
                    _countdownDisplayService.RelocateWindow();
                    break;

                case MonitorChangeDescription.WindowToMonitor:
                case MonitorChangeDescription.NoneToMonitor:
                    _countdownDisplayService.OpenWindowInMonitor();
                    break;

                case MonitorChangeDescription.MonitorToWindow:
                case MonitorChangeDescription.NoneToWindow:
                    _countdownDisplayService.OpenWindowWindowed();
                    break;

                case MonitorChangeDescription.WindowToNone:
                case MonitorChangeDescription.MonitorToNone:
                    _countdownDisplayService.Hide();
                    break;

                default:
                    throw new NotImplementedException();
            }

            OnPropertyChanged(nameof(AlwaysOnTop));
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not change monitor";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    private async Task LaunchTimerWindowAsync()
    {
        if (_optionsService.CanDisplayTimerWindow)
        {
            // on launch we display the timer window after a short delay (for aesthetics only)
            await Task.Delay(1000).ConfigureAwait(true);

            await Application.Current.Dispatcher.BeginInvoke(OpenTimerWindow);
        }
    }

    private void InitHeartbeatTimer()
    {
        _heartbeatTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _heartbeatTimer.Tick += HeartbeatTimerTick;
        _heartbeatTimer.Start();
    }

    private void HeartbeatTimerTick(object? sender, System.EventArgs e)
    {
        _heartbeatTimer.Stop();
        try
        {
            ManageCountdownOnHeartbeat();
            ManageScheduleOnHeartbeat();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Error during heartbeat");
        }
        finally
        {
            _heartbeatTimer.Start();
        }
    }

    private void ManageScheduleOnHeartbeat()
    {
        if ((_dateTimeService.Now() - _lastRefreshedSchedule).Seconds > 10)
        {
            _lastRefreshedSchedule = _dateTimeService.Now();
            WeakReferenceMessenger.Default.Send(new RefreshScheduleMessage());
        }
    }

    private void ManageCountdownOnHeartbeat()
    {
        if (_optionsService.CanDisplayCountdownWindow && 
            !CountDownActive && 
            !_countdownDisplayService.IsCountdownDone && 
            _countdownTimerTriggerService.IsInCountdownPeriod(out var secondsOffset))
        {
            StartCountdown(secondsOffset);
        }
    }

    private void OnStopCountdown(object recipient, StopCountDownMessage message)
    {
        _countdownDisplayService.Stop(true);
    }
        
    /// <summary>
    /// Responds to the NavigateMessage and swaps out one page for another.
    /// </summary>
    /// <param name="message">NavigateMessage message.</param>
    private void OnNavigate(object recipient, NavigateMessage message)
    {
        if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
        {
            // we only init the settings page when first used...
            InitSettingsPage();
        }

        CurrentPage = _pages[message.TargetPageName];
        CurrentPageName = message.TargetPageName;

        var page = (IPage)CurrentPage.DataContext;
        page.Activated(message.State);
    }

    /// <summary>
    /// If the timer window is open when we change the timer display then relocate it;
    /// otherwise open it
    /// </summary>
    private void RelocateTimerWindow()
    {
        if (_timerOutputDisplayService.IsWindowAvailable())
        {
            _timerOutputDisplayService.RelocateWindow();
        }
        else
        {
            OpenTimerWindow();
        }
    }

    private void OpenTimerWindow()
    {
        try
        {
            if (_optionsService.Options.MainMonitorIsWindowed)
            {
                _timerOutputDisplayService.OpenWindowWindowed();                    
            }
            else
            {
                _timerOutputDisplayService.OpenWindowInMonitor();
                OnPropertyChanged(nameof(AlwaysOnTop));
            }
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not open timer window";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    private void CloseTimerWindow()
    {
        try
        {
            _timerOutputDisplayService.Close();
            OnPropertyChanged(nameof(AlwaysOnTop));
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not close timer window";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    /// <summary>
    /// Starts the countdown (pre-meeting) timer
    /// </summary>
    /// <param name="offsetSeconds">
    /// The offset in seconds (the timer already started offsetSeconds ago).
    /// </param>
    private void StartCountdown(int offsetSeconds)
    {
        if (_optionsService.CanDisplayCountdownWindow)
        {
            Log.Logger.Information("Launching countdown timer");
            EventTracker.Track(EventName.CountdownTimer);

            _countdownDisplayService.Start(offsetSeconds);

            var launched = _optionsService.Options.CountdownMonitorIsWindowed 
                ? _countdownDisplayService.OpenWindowWindowed() 
                : _countdownDisplayService.OpenWindowInMonitor();

            if (launched)
            {
                Task.Delay(1000).ContinueWith(_ =>
                {
                    if (CountdownAndTimerShareSameMonitor())
                    {
                        // timer monitor and countdown monitor are the same.

                        // hide the timer window after a short delay (so that it doesn't appear 
                        // as another top-level window during alt-TAB)...
                        Application.Current.Dispatcher.BeginInvoke(_timerOutputDisplayService.HideWindow);
                    }
                });
            }
        }
    }

    private bool CountdownAndTimerShareSameMonitor()
    {
        if (_optionsService.Options.MainMonitorIsWindowed || _optionsService.Options.CountdownMonitorIsWindowed)
        {
            return false;
        }

        return _optionsService.Options.TimerMonitorId == _optionsService.Options.CountdownMonitorId;
    }

    private static bool ForceSoftwareRendering()
    {
        // https://blogs.msdn.microsoft.com/jgoldb/2010/06/22/software-rendering-usage-in-wpf/
        // renderingTier values:
        // 0 => No graphics hardware acceleration available for the application on the device
        //      and DirectX version level is less than version 7.0
        // 1 => Partial graphics hardware acceleration available on the video card. This 
        //      corresponds to a DirectX version that is greater than or equal to 7.0 and 
        //      less than 9.0.
        // 2 => A rendering tier value of 2 means that most of the graphics features of WPF 
        //      should use hardware acceleration provided the necessary system resources have 
        //      not been exhausted. This corresponds to a DirectX version that is greater 
        //      than or equal to 9.0.
        var renderingTier = RenderCapability.Tier >> 16;
        return renderingTier == 0;
    }
}