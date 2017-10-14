using System;
using GalaSoft.MvvmLight;
using System.Windows;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.ViewModel.Messages;
using OnlyT.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using OnlyT.EventArgs;
using OnlyT.Services.Bell;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Services.Timer;
using OnlyT.Utils;
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
        private readonly IOptionsService _optionsService;
        private readonly IMonitorsService _monitorsService;
        private readonly IBellService _bellService;
        private readonly ITalkTimerService _timerService;
        private readonly IHttpServer _httpServer;
        private string _currentPageName;
        private readonly TimerOutputWindowViewModel _timerWindowViewModel;

        public MainViewModel(
           IOptionsService optionsService,
           IMonitorsService monitorsService,
           ITalkTimerService timerService,
           IHttpServer httpServer,
           IBellService bellService)
        {
            _optionsService = optionsService;
            _monitorsService = monitorsService;
            _bellService = bellService;
            _httpServer = httpServer;
            _timerService = timerService;

            // subscriptions...
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
            Messenger.Default.Register<TimerMonitorChangedMessage>(this, OnTimerMonitorChanged);
            Messenger.Default.Register<AlwaysOnTopChangedMessage>(this, OnAlwaysOnTopChanged);
            Messenger.Default.Register<OvertimeMessage>(this, OnTalkOvertime);

            InitHttpServer();

            // should really create a "page service" rather than create views in the main view model!
            _pages.Add(OperatorPageViewModel.PageName, new OperatorPage());
            _pages.Add(SettingsPageViewModel.PageName, new SettingsPage());

            _timerWindowViewModel = new TimerOutputWindowViewModel(_optionsService);

            Messenger.Default.Send(new NavigateMessage(null, OperatorPageViewModel.PageName, null));

#pragma warning disable 4014
            // (fire and forget)
            LaunchTimerWindowAsync();
#pragma warning restore 4014
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
            }
        }

        /// <summary>
        /// Responds to the NavigateMessage and swaps out one page for another
        /// </summary>
        /// <param name="message"></param>
        private void OnNavigate(NavigateMessage message)
        {
            CurrentPage = _pages[message.TargetPageName];
            _currentPageName = message.TargetPageName;
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
                Messenger.Default.Send(new ShutDownMessage(_currentPageName));
                CloseTimerWindow();
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
                var timerMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
                if (timerMonitor != null)
                {
                    _timerWindow.Hide();
                    _timerWindow.WindowState = WindowState.Normal;

                    var area = timerMonitor.Monitor.WorkingArea;
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

        private void OpenTimerWindow()
        {
            try
            {
                var timerMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
                if (timerMonitor != null)
                {
                    _timerWindow = new TimerOutputWindow {DataContext = _timerWindowViewModel};

                    var area = timerMonitor.Monitor.WorkingArea;
                    _timerWindow.Left = area.Left;
                    _timerWindow.Top = area.Top;
                    _timerWindow.Width = 0;
                    _timerWindow.Height = 0;

                    _timerWindow.Topmost = true;
                    _timerWindow.Show();
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not open timer window");
            }
        }

        private void HideTimerWindow()
        {
            _timerWindow?.Hide();
        }

        private void CloseTimerWindow()
        {
            try
            {
                if (_timerWindow != null)
                {
                    _timerWindow.Close();
                    _timerWindow = null;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not close timer window");
            }
        }
    }
}