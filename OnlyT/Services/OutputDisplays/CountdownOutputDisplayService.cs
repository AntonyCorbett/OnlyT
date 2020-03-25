namespace OnlyT.Services.OutputDisplays
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.Services.Monitors;
    using OnlyT.Services.Options;
    using OnlyT.ViewModel;
    using OnlyT.ViewModel.Messages;
    using OnlyT.Windows;
    using Serilog;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class CountdownOutputDisplayService : OutputDisplayServiceBase, ICountdownOutputDisplayService
    {
        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ITimerOutputDisplayService _timerOutputDisplayService;
        private CountdownWindow _countdownWindow;

        public CountdownOutputDisplayService(
            IMonitorsService monitorsService,
            IOptionsService optionsService,
            IDateTimeService dateTimeService,
            ITimerOutputDisplayService timerOutputDisplayService)
            : base(optionsService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;
            _dateTimeService = dateTimeService;
            _timerOutputDisplayService = timerOutputDisplayService;
        }

        public bool IsCountdownDone { get; private set; }
        
        public bool IsCountingDown { get; private set; }

        public bool IsWindowAvailable() => _countdownWindow != null;

        public bool IsWindowVisible() => _countdownWindow != null && _countdownWindow.IsVisible;
        
        public void Activate()
        {
            _countdownWindow.Activate();
        }

        public void RelocateWindow()
        {
            if (IsWindowAvailable())
            {
                RelocateWindow(_countdownWindow, _monitorsService.GetMonitorItem(_optionsService.Options.CountdownMonitorId));
            }
        }

        public bool OpenWindowInMonitor()
        {
            EnsureCountdownWindowExists();
            
            if (IsCountingDown)
            { 
                try
                {
                    var targetMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.CountdownMonitorId);
                    if (targetMonitor != null)
                    {
                        ConfigureForMonitorOperation();
                        ShowWindowFullScreenOnTop(_countdownWindow, targetMonitor);
                        
                        Messenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = true });
                        Messenger.Default.Send(new BringMainWindowToFrontMessage());

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

        public bool OpenWindowWindowed()
        {
            EnsureCountdownWindowExists();

            if (IsCountingDown)
            {
                try
                {
                    ConfigureForWindowedOperation();

                    _countdownWindow.Show();
                    _countdownWindow.AdjustWindowPositionAndSize();

                    Messenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = true });

                    return true;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Could not open countdown window");
                }
            }

            return false;
        }

        public void Start(int offsetSeconds)
        {
            EnsureCountdownWindowExists();
            
            IsCountingDown = true;

            _countdownWindow.Start(offsetSeconds);
        }

        public void SaveWindowedPos()
        {
            _countdownWindow?.SaveWindowPos();
        }

        public void Hide()
        {
            _countdownWindow?.Hide();
        }

        public void Stop()
        {
            IsCountdownDone = true;
            IsCountingDown = false;

            Messenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = false });

            if (_optionsService.CanDisplayTimerWindow)
            {
                _timerOutputDisplayService.ShowWindow();
            }

            Task.Delay(1000).ContinueWith(t =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(Close);
            });
        }

        public void Close()
        {
            try
            {
                if (_countdownWindow != null)
                {
                    _countdownWindow.TimeUpEvent -= OnCountdownTimeUp;
                }

                Log.Logger.Debug("Closing countdown window");

                _countdownWindow?.Close();
                _countdownWindow = null;

                BringJwlToFront();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not close countdown window");
            }
        }

        private void OnCountdownTimeUp(object sender, System.EventArgs e)
        {
            Stop();
        }

        private void ConfigureForWindowedOperation()
        {
            var dataContext = (CountdownTimerViewModel)_countdownWindow.DataContext;
            dataContext.WindowedOperation = true;

            _countdownWindow.WindowState = WindowState.Normal;

            _countdownWindow.MinHeight = 300;
            _countdownWindow.MinWidth = 400;

            _countdownWindow.ResizeMode = ResizeMode.CanResize;
            _countdownWindow.ShowInTaskbar = true;
            _countdownWindow.WindowStyle = WindowStyle.SingleBorderWindow;

            _countdownWindow.Topmost = false;
        }

        private void ConfigureForMonitorOperation()
        {
            var dataContext = (CountdownTimerViewModel)_countdownWindow.DataContext;
            dataContext.WindowedOperation = false;
            
            _countdownWindow.ResizeMode = ResizeMode.NoResize;
            _countdownWindow.ShowInTaskbar = false;
            _countdownWindow.WindowStyle = WindowStyle.None;

            _countdownWindow.Topmost = true;
        }

        private void EnsureCountdownWindowExists()
        {
            if (_countdownWindow == null)
            {
                _countdownWindow = new CountdownWindow(_optionsService, _dateTimeService);
                _countdownWindow.TimeUpEvent += OnCountdownTimeUp;
            }
        }
    }
}
