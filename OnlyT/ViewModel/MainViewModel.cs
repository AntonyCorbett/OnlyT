namespace OnlyT.ViewModel
{
    // ReSharper disable CatchAllClause
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Forms;
    using System.Windows.Threading;
    using EventArgs;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;
    using Messages;
    using Models;
    using Serilog;
    using Services.CountdownTimer;
    using Services.Monitors;
    using Services.Options;
    using Services.Timer;
    using Utils;
    using WebServer;
    using Windows;

    /// <summary>
    /// View model for the main page (which is a placeholder for the Operator or Settings page)
    /// </summary>
    /// <remarks>Needs refactoring to move _timerWindow and _countdownWindow into a "window service"</remarks>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class MainViewModel : ViewModelBase
    {
        private readonly Dictionary<string, FrameworkElement> _pages = new Dictionary<string, FrameworkElement>();
        private readonly IOptionsService _optionsService;
        private readonly IMonitorsService _monitorsService;
        private readonly ICountdownTimerTriggerService _countdownTimerTriggerService;
        private readonly ITalkTimerService _timerService;
        private readonly IHttpServer _httpServer;
        private readonly TimerOutputWindowViewModel _timerWindowViewModel;
        private readonly CountdownTimerViewModel _countdownWindowViewModel;
        private DispatcherTimer _heartbeatTimer;
        private bool _countdownDone;
        private TimerOutputWindow _timerWindow;
        private CountdownWindow _countdownWindow;
        private (int dpiX, int dpiY) _systemDpi;

        public MainViewModel(
           IOptionsService optionsService,
           IMonitorsService monitorsService,
           ITalkTimerService timerService,
           IHttpServer httpServer,
           ICountdownTimerTriggerService countdownTimerTriggerService)
        {
            _optionsService = optionsService;
            _monitorsService = monitorsService;
            _httpServer = httpServer;
            _timerService = timerService;
            _countdownTimerTriggerService = countdownTimerTriggerService;

            _httpServer.RequestForTimerDataEvent += OnRequestForTimerData;

            _systemDpi = WindowPlacement.GetDpiSettings();

            // subscriptions...
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
            Messenger.Default.Register<TimerMonitorChangedMessage>(this, OnTimerMonitorChanged);
            Messenger.Default.Register<AlwaysOnTopChangedMessage>(this, OnAlwaysOnTopChanged);
            Messenger.Default.Register<HttpServerChangedMessage>(this, OnHttpServerChanged);
            Messenger.Default.Register<StopCountDownMessage>(this, OnStopCountdown);
            
            InitHttpServer();

            // should really create a "page service" rather than create views in the main view model!
            _pages.Add(OperatorPageViewModel.PageName, new OperatorPage());

            _timerWindowViewModel = new TimerOutputWindowViewModel(_optionsService);
            _countdownWindowViewModel = new CountdownTimerViewModel();

            Messenger.Default.Send(new NavigateMessage(null, OperatorPageViewModel.PageName, null));

#pragma warning disable 4014
            // (fire and forget)
            LaunchTimerWindowAsync();
#pragma warning restore 4014
        }

        public void Closing(CancelEventArgs e)
        {
            e.Cancel = _timerService.IsRunning;
            if (!e.Cancel)
            {
                Messenger.Default.Send(new ShutDownMessage(CurrentPageName));
                CloseTimerWindow();
                CloseCountdownWindow();
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
            if (!IsInDesignMode && _optionsService.IsTimerMonitorSpecified)
            {
                Log.Logger.Information("Launching countdown timer");

                if (OpenCountdownWindow(offsetSeconds))
                {
                    Task.Delay(1000).ContinueWith(t =>
                    {
                        // hide the timer window after a short delay (so that it doesn't appear 
                        // as another top-level window during alt-TAB)...
                        DispatcherHelper.CheckBeginInvokeOnUI(HideTimerWindow);
                    });
                }
            }
        }

        public string CurrentPageName { get; private set; }

        private void InitSettingsPage()
        {
            // we only init the settings page when first used.
            if (!_pages.ContainsKey(SettingsPageViewModel.PageName))
            {
                _pages.Add(SettingsPageViewModel.PageName, new SettingsPage());
            }
        }

        private void OnHttpServerChanged(HttpServerChangedMessage msg)
        {
            _httpServer.Stop();
            InitHttpServer();
        }

        private void InitHttpServer()
        {
            if (_optionsService.Options.IsWebClockEnabled || _optionsService.Options.IsApiEnabled)
            {
                _httpServer.Start(_optionsService.Options.HttpServerPort);
            }
        }

        private void OnRequestForTimerData(object sender, TimerInfoEventArgs timerData)
        {
            // we received a web request for the timer clock info...
            var info = _timerService.GetClockRequestInfo();

            if (info == null || !info.IsRunning)
            {
                timerData.Mode = ClockServerMode.TimeOfDay;
            }
            else
            {
                timerData.Mode = ClockServerMode.Timer;

                timerData.TargetSecs = info.TargetSeconds;
                timerData.Mins = info.ElapsedTime.Minutes;
                timerData.Secs = info.ElapsedTime.Seconds;
                timerData.Millisecs = info.ElapsedTime.Milliseconds;
            }
        }

        /// <summary>
        /// Responds to change in the application's "Always on top" option.
        /// </summary>
        /// <param name="message">AlwaysOnTopChangedMessage message.</param>
        private void OnAlwaysOnTopChanged(AlwaysOnTopChangedMessage message)
        {
            RaisePropertyChanged(nameof(AlwaysOnTop));
        }

        /// <summary>
        /// Responds to a change in timer monitor.
        /// </summary>
        /// <param name="message">TimerMonitorChangedMessage message.</param>
        private void OnTimerMonitorChanged(TimerMonitorChangedMessage message)
        {
            try
            {
                if (_optionsService.IsTimerMonitorSpecified)
                {
                    RelocateTimerWindow();
                }
                else
                {
                    HideTimerWindow();
                }

                RaisePropertyChanged(nameof(AlwaysOnTop));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not change monitor");
            }
        }

        private async Task LaunchTimerWindowAsync()
        {
            if (!IsInDesignMode && _optionsService.IsTimerMonitorSpecified)
            {
                // on launch we display the timer window after a short delay (for aesthetics only)
                await Task.Delay(1000).ConfigureAwait(true);
                OpenTimerWindow();
                InitHeartbeatTimer();
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

        private void HeartbeatTimerTick(object sender, EventArgs e)
        {
            _heartbeatTimer.Stop();
            try
            {
                if (_optionsService.Options.IsCountdownEnabled)
                {
                    if (!CountDownActive &&
                        !_countdownDone &&
                        _countdownTimerTriggerService.IsInCountdownPeriod(out var secondsOffset))
                    {
                        StartCountdown(secondsOffset);
                    }
                }
                else
                {
                    // countdown not enabled...
                    if (CountDownActive)
                    {
                        StopCountdown();
                    }
                }
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

        private void OnStopCountdown(StopCountDownMessage message)
        {
            StopCountdown();
        }

        private void StopCountdown()
        {
            _countdownDone = true;

            Messenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = false });

            _timerWindow?.Show();

            Task.Delay(1000).ContinueWith(t =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(CloseCountdownWindow);
            });
        }

        /// <summary>
        /// Responds to the NavigateMessage and swaps out one page for another.
        /// </summary>
        /// <param name="message">NavigateMessage message.</param>
        private void OnNavigate(NavigateMessage message)
        {
            if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
            {
                // we only init the settings page when first used...
                InitSettingsPage();
            }

            CurrentPage = _pages[message.TargetPageName];
            CurrentPageName = message.TargetPageName;
            ((IPage)CurrentPage.DataContext).Activated(message.State);
        }

        private FrameworkElement _currentPage;

        public FrameworkElement CurrentPage
        {
            get => _currentPage;
            set
            {
                if (!ReferenceEquals(_currentPage, value))
                {
                    _currentPage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool AlwaysOnTop
        {
            get
            {
                var result = _optionsService.Options.AlwaysOnTop ||
                             (_timerWindow != null && _timerWindow.IsVisible) ||
                             (_countdownWindow != null && _countdownWindow.IsVisible);

                return result;
            }
        }

        /// <summary>
        /// If the timer window is open when we change the timer display then relocate it;
        /// otherwise open it
        /// </summary>
        private void RelocateTimerWindow()
        {
            if (_timerWindow != null)
            {
                var targetMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
                if (targetMonitor != null)
                {
                    _timerWindow.Hide();
                    _timerWindow.WindowState = WindowState.Normal;

                    LocateWindowAtOrigin(_timerWindow, targetMonitor.Monitor);
                    
                    _timerWindow.Topmost = true;
                    _timerWindow.WindowState = WindowState.Maximized;
                    _timerWindow.Show();
                }
            }
            else
            {
                OpenTimerWindow();
            }
        }
        
        //// private bool InSettingsPage => CurrentPageName.Equals(SettingsPageViewModel.PageName);

        private bool CountDownActive => _countdownWindow != null;

        private bool OpenCountdownWindow(int offsetSeconds)
        {
            if (!CountDownActive)
            {
                try
                {
                    var targetMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
                    if (targetMonitor != null)
                    {
                        _countdownWindow = new CountdownWindow { DataContext = _countdownWindowViewModel };
                        _countdownWindow.TimeUpEvent += OnCountdownTimeUp;
                        ShowWindowFullScreenOnTop(_countdownWindow, targetMonitor);
                        _countdownWindow.Start(offsetSeconds);

                        Messenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = true });

                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not open countdown window");
                }
            }

            return false;
        }

        private void OnCountdownTimeUp(object sender, EventArgs e)
        {
            StopCountdown();
        }

        private void OpenTimerWindow()
        {
            try
            {
                var targetMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
                if (targetMonitor != null)
                {
                    _timerWindow = new TimerOutputWindow(_optionsService) { DataContext = _timerWindowViewModel };
                    ShowWindowFullScreenOnTop(_timerWindow, targetMonitor);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not open timer window");
            }
        }

        private void LocateWindowAtOrigin(Window window, Screen monitor)
        {
            var area = monitor.WorkingArea;

            var left = (area.Left * 96) / _systemDpi.dpiX;
            var top = (area.Top * 96) / _systemDpi.dpiY;

            // these seemingly redundant sizing statements are required!
            window.Left = 0;
            window.Top = 0;
            window.Width = 0;
            window.Height = 0;

            window.Left = left;
            window.Top = top;
        }

        private void ShowWindowFullScreenOnTop(Window window, MonitorItem monitor)
        {
            LocateWindowAtOrigin(window, monitor.Monitor);

            window.Topmost = true;
            window.Show();

            RaisePropertyChanged(nameof(AlwaysOnTop));
        }

        private void HideTimerWindow()
        {
            _timerWindow?.Hide();
        }

        private void CloseTimerWindow()
        {
            try
            {
                _timerWindow?.Close();
                _timerWindow = null;

                RaisePropertyChanged(nameof(AlwaysOnTop));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not close timer window");
            }
        }

        private void CloseCountdownWindow()
        {
            try
            {
                if (_countdownWindow != null)
                {
                    _countdownWindow.TimeUpEvent -= OnCountdownTimeUp;
                }

                _countdownWindow?.Close();
                _countdownWindow = null;

                RaisePropertyChanged(nameof(AlwaysOnTop));
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not close countdown window");
            }
        }
    }
}