using System;
using GalaSoft.MvvmLight;
using System.Windows;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.ViewModel.Messages;
using OnlyT.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;
using OnlyT.EventArgs;
using OnlyT.Models;
using OnlyT.Services.Bell;
using OnlyT.Services.CountdownTimer;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Services.Timer;
using OnlyT.WebServer;
using Serilog;

namespace OnlyT.ViewModel
{
    /// <summary>
    /// View model for the main page (which is a placeholder for the Operator or Settings page)
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly Dictionary<string, FrameworkElement> _pages = new Dictionary<string, FrameworkElement>();
        private TimerOutputWindow _timerWindow;
        private CountdownWindow _countdownWindow;
        private readonly IOptionsService _optionsService;
        private readonly IMonitorsService _monitorsService;
        private readonly IBellService _bellService;
        private readonly ICountdownTimerTriggerService _countdownTimerTriggerService;
        private readonly ITalkTimerService _timerService;
        private readonly IHttpServer _httpServer;
        private readonly TimerOutputWindowViewModel _timerWindowViewModel;
        private readonly CountdownTimerViewModel _countdownWindowViewModel;
        private DispatcherTimer _heartbeatTimer;
        private bool _countdownDone;

        public MainViewModel(
           IOptionsService optionsService,
           IMonitorsService monitorsService,
           ITalkTimerService timerService,
           IHttpServer httpServer,
           IBellService bellService,
           ICountdownTimerTriggerService countdownTimerTriggerService)
        {
            _optionsService = optionsService;
            _monitorsService = monitorsService;
            _bellService = bellService;
            _httpServer = httpServer;
            _timerService = timerService;
            _countdownTimerTriggerService = countdownTimerTriggerService;

            // subscriptions...
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
            Messenger.Default.Register<TimerMonitorChangedMessage>(this, OnTimerMonitorChanged);
            Messenger.Default.Register<AlwaysOnTopChangedMessage>(this, OnAlwaysOnTopChanged);
            Messenger.Default.Register<OvertimeMessage>(this, OnTalkOvertime);
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

        /// <summary>
        /// Starts the countdown (pre-meeting) timer
        /// </summary>
        private void StartCountdown(int offsetSeconds)
        {
            if (!IsInDesignMode && _optionsService.IsTimerMonitorSpecified)
            {
                Log.Logger.Information("Launching countdown timer");
                
                OpenCountdownWindow(offsetSeconds);
                
                Task.Delay(1000).ContinueWith(t =>
                {
                    // hide the timer window after a short delay (so that it doesn't appear 
                    // as another top-level window during alt-TAB)...
                    DispatcherHelper.CheckBeginInvokeOnUI(HideTimerWindow);
                });
            }
        }

        public string CurrentPageName { get; set; }
        
        private void InitSettingsPage()
        {
            // we only init the settings page when first used...
            
            if (!_pages.ContainsKey(SettingsPageViewModel.PageName))
            {
                _pages.Add(SettingsPageViewModel.PageName, new SettingsPage());
            }
        }

        private void OnHttpServerChanged(HttpServerChangedMessage msg)
        {
            _httpServer.Stop();

            if (_optionsService.Options.IsWebClockEnabled)
            {
                _httpServer.Start(_optionsService.Options.HttpServerPort);
            }
        }

        private void InitHttpServer()
        {
            _httpServer.ClockServerRequestHandler += ClockRequestHandler;
            if (_optionsService.Options.IsWebClockEnabled)
            {
                _httpServer.Start(_optionsService.Options.HttpServerPort);
            }
        }

        private void ClockRequestHandler(object sender, ClockServerEventArgs e)
        {
            // we received a web request for the timer clock info...

            TimerChangedEventArgs info = _timerService.GetClockRequestInfo();
            if (info == null || !info.IsRunning)
            {
                e.Mode = ClockServerMode.TimeOfDay;
            }
            else
            {
                e.Mode = ClockServerMode.Timer;
                e.Mins = info.ElapsedSecs / 60;
                e.Secs = info.ElapsedSecs % 60;
                e.TargetSecs = info.TargetSecs;
            }
        }

        private void OnTalkOvertime(OvertimeMessage message)
        {
            if (message.UseBellForTalk && _optionsService.Options.IsBellEnabled)
            {
                _bellService.Play(_optionsService.Options.BellVolumePercent);
            }
        }

        /// <summary>
        /// Responds to change in the application's "Always on top" option
        /// </summary>
        /// <param name="message"></param>
        private void OnAlwaysOnTopChanged(AlwaysOnTopChangedMessage message)
        {
            RaisePropertyChanged(nameof(AlwaysOnTop));
        }

        /// <summary>
        /// Responds to a change in timer monitor
        /// </summary>
        /// <param name="message"></param>
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
            _heartbeatTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
            _heartbeatTimer.Interval = TimeSpan.FromSeconds(1);
            _heartbeatTimer.Tick += HeartbeatTimerTick;
            _heartbeatTimer.Start();
        }

        private void HeartbeatTimerTick(object sender, System.EventArgs e)
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
        /// Responds to the NavigateMessage and swaps out one page for another
        /// </summary>
        /// <param name="message"></param>
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

        public bool AlwaysOnTop => _optionsService.Options.AlwaysOnTop;

        public void Closing(object sender, CancelEventArgs e)
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

                    var area = targetMonitor.Monitor.WorkingArea;
                    _timerWindow.Left = area.Left;
                    _timerWindow.Top = area.Top;

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

        private bool CountDownActive => _countdownWindow != null;

        private void OpenCountdownWindow(int offsetSeconds)
        {
            if (!CountDownActive)
            {
                try
                {
                    var targetMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
                    if (targetMonitor != null)
                    {
                        _countdownWindow = new CountdownWindow {DataContext = _countdownWindowViewModel};
                        _countdownWindow.TimeUpEvent += OnCountdownTimeUp;
                        ShowWindowFullScreenOnTop(_countdownWindow, targetMonitor);
                        _countdownWindow.Start(offsetSeconds);

                        Messenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = true });
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not open countdown window");
                }
            }
        }

        private void OnCountdownTimeUp(object sender, System.EventArgs e)
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
                    _timerWindow = new TimerOutputWindow {DataContext = _timerWindowViewModel};
                    ShowWindowFullScreenOnTop(_timerWindow, targetMonitor);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not open timer window");
            }
        }

        private void ShowWindowFullScreenOnTop(Window window, MonitorItem monitor)
        {
            var area = monitor.Monitor.WorkingArea;
            
            window.Left = area.Left;
            window.Top = area.Top;
            window.Width = 0;
            window.Height = 0;

            window.Topmost = true;
            window.Show();
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
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not close countdown window");
            }
        }
    }
}