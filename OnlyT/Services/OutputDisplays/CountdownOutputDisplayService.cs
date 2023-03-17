using Microsoft.Toolkit.Mvvm.Messaging;
using System;
using System.Threading.Tasks;
using System.Windows;
using OnlyT.Common.Services.DateTime;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Services.Snackbar;
using OnlyT.ViewModel;
using OnlyT.ViewModel.Messages;
using OnlyT.Windows;
using Serilog;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OnlyT.Services.OutputDisplays
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class CountdownOutputDisplayService : OutputDisplayServiceBase, ICountdownOutputDisplayService
    {
        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ITimerOutputDisplayService _timerOutputDisplayService;
        private readonly ISnackbarService _snackbarService;
        private CountdownWindow? _countdownWindow;

        public CountdownOutputDisplayService(
            IMonitorsService monitorsService,
            IOptionsService optionsService,
            IDateTimeService dateTimeService,
            ITimerOutputDisplayService timerOutputDisplayService,
            ISnackbarService snackbarService)
            : base(optionsService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;
            _dateTimeService = dateTimeService;
            _timerOutputDisplayService = timerOutputDisplayService;
            _snackbarService = snackbarService;
        }

        public bool IsCountdownDone { get; private set; }
        
        public bool IsCountingDown { get; private set; }

        public bool IsWindowAvailable() => _countdownWindow != null;

        public bool IsWindowVisible() => _countdownWindow != null && _countdownWindow.IsVisible;
        
        public void Activate()
        {
            _countdownWindow?.Activate();
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

                        WeakReferenceMessenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = true });
                        WeakReferenceMessenger.Default.Send(new BringMainWindowToFrontMessage());

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
                    if (_countdownWindow!.AllowsTransparency)
                    {
                        // we can't convert the window to WindowStyle.SingleBorderWindow since it has
                        // "AllowsTransparency" set. Warn the user and back out.
                        _snackbarService.EnqueueWithOk(Properties.Resources.COUNTDOWN_STYLE_CONFLICT);
                        return false;
                    }

                    ConfigureForWindowedOperation();

                    _countdownWindow.Topmost = true;
                    _countdownWindow.Show();
                    _countdownWindow.AdjustWindowPositionAndSize();

                    WeakReferenceMessenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = true });

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

            _countdownWindow!.Start(offsetSeconds);
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

            WeakReferenceMessenger.Default.Send(new CountdownWindowStatusChangedMessage { Showing = false });

            if (_optionsService.CanDisplayTimerWindow)
            {
                _timerOutputDisplayService.ShowWindow();
            }

            Task.Delay(1000).ContinueWith(_ => Application.Current.Dispatcher.BeginInvoke(new Action(Close)));
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

        private void OnCountdownTimeUp(object? sender, System.EventArgs e)
        {
            Stop();
        }

        private void ConfigureForWindowedOperation()
        {
            CheckCountdownWindow();

            var dataContext = (CountdownTimerViewModel)_countdownWindow!.DataContext;
            dataContext.WindowedOperation = true;

            _countdownWindow.WindowState = WindowState.Normal;

            _countdownWindow.MinHeight = 100;
            _countdownWindow.MinWidth = 200;

            _countdownWindow.ResizeMode = ResizeMode.CanResize;
            _countdownWindow.ShowInTaskbar = true;
            _countdownWindow.WindowStyle = WindowStyle.None;

            _countdownWindow.Topmost = false;
        }

        private void CheckCountdownWindow()
        {
            if (_countdownWindow == null)
            {
                throw new Exception("Countdown window null");
            }
        }

        private void ConfigureForMonitorOperation()
        {
            CheckCountdownWindow();

            var dataContext = (CountdownTimerViewModel)_countdownWindow!.DataContext;
            dataContext.WindowedOperation = false;

            _countdownWindow.WindowState = WindowState.Normal;
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

                if (_optionsService.Options.IsCountdownWindowTransparent &&
                    !_optionsService.Options.CountdownMonitorIsWindowed)
                {
                    _countdownWindow.AllowsTransparency = true;
                }

                _countdownWindow.TimeUpEvent += OnCountdownTimeUp;
            }
        }
    }
}
