using Microsoft.Toolkit.Mvvm.Messaging;

namespace OnlyT.Services.OutputDisplays
{
    using System.Windows;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.Services.Monitors;
    using OnlyT.Services.Options;
    using OnlyT.ViewModel;
    using OnlyT.ViewModel.Messages;
    using OnlyT.Windows;
    using Serilog;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class TimerOutputDisplayService : OutputDisplayServiceBase, ITimerOutputDisplayService
    {
        private readonly IMonitorsService _monitorsService;
        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;
        private TimerOutputWindow? _timerWindow;

        public TimerOutputDisplayService(
            IMonitorsService monitorsService, 
            IOptionsService optionsService,
            IDateTimeService dateTimeService)
            : base(optionsService)
        {
            _monitorsService = monitorsService;
            _optionsService = optionsService;
            _dateTimeService = dateTimeService;
        }

        public bool IsWindowAvailable() => _timerWindow != null;
        
        public bool IsWindowVisible() => _timerWindow != null && _timerWindow.IsVisible;

        public void ShowWindow()
        {
            _timerWindow?.Show();
        }

        public void RelocateWindow()
        {
            var monitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
            if (monitor != null)
            {
                Log.Logger.Debug("Relocating timer window to: {MonitorName}", monitor.FriendlyName);
                RelocateWindow(_timerWindow, monitor);
            }
        }

        public void OpenWindowInMonitor()
        {
            var targetMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
            if (targetMonitor != null)
            {
                _timerWindow ??= new TimerOutputWindow(_optionsService, _dateTimeService);

                ConfigureForMonitorOperation();

                ShowWindowFullScreenOnTop(_timerWindow, targetMonitor);

                WeakReferenceMessenger.Default.Send(new BringMainWindowToFrontMessage());
            }
        }

        public void SaveWindowedPos()
        {
            _timerWindow?.SaveWindowPos();
        }

        public void OpenWindowWindowed()
        {
            if (_timerWindow == null)
            {
                _timerWindow = new TimerOutputWindow(_optionsService, _dateTimeService);
            }

            ConfigureForWindowedOperation();

            _timerWindow.Show();

            _timerWindow.AdjustWindowPositionAndSize();
        }

        public void HideWindow()
        {
            _timerWindow?.Hide();
        }

        public void Close()
        {
            Log.Logger.Debug("Closing timer window");

            _timerWindow?.Close();
            _timerWindow = null;
        }

        private void ConfigureForMonitorOperation()
        {
            var dataContext = (TimerOutputWindowViewModel)_timerWindow!.DataContext;
            dataContext.WindowedOperation = false;

            _timerWindow.WindowState = WindowState.Normal;
            _timerWindow.ResizeMode = ResizeMode.NoResize;
            _timerWindow.ShowInTaskbar = false;
            _timerWindow.WindowStyle = WindowStyle.None;

            _timerWindow.Topmost = true;
        }

        private void ConfigureForWindowedOperation()
        {
            var dataContext = (TimerOutputWindowViewModel)_timerWindow!.DataContext;
            dataContext.WindowedOperation = true;

            _timerWindow.WindowState = WindowState.Normal;

            _timerWindow.MinHeight = 300;
            _timerWindow.MinWidth = 400;
            
            _timerWindow.ResizeMode = ResizeMode.CanResize;
            _timerWindow.ShowInTaskbar = true;
            _timerWindow.WindowStyle = WindowStyle.None;

            _timerWindow.Topmost = false;
        }
    }
}
