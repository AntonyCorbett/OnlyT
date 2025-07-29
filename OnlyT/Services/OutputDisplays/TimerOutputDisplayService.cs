using CommunityToolkit.Mvvm.Messaging;
using OnlyT.Common.Services.DateTime;
using OnlyT.Services.CommandLine;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.ViewModel;
using OnlyT.ViewModel.Messages;
using OnlyT.Windows;
using Serilog;
using Serilog.Events;
using System.Windows;

namespace OnlyT.Services.OutputDisplays;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class TimerOutputDisplayService : OutputDisplayServiceBase, ITimerOutputDisplayService
{
    private readonly IMonitorsService _monitorsService;
    private readonly IOptionsService _optionsService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICommandLineService _commandLineService;
    private TimerOutputWindow? _timerWindow;

    public TimerOutputDisplayService(
        IMonitorsService monitorsService, 
        IOptionsService optionsService,
        IDateTimeService dateTimeService,
        ICommandLineService commandLineService)
        : base(optionsService)
    {
        _monitorsService = monitorsService;
        _optionsService = optionsService;
        _dateTimeService = dateTimeService;
        _commandLineService = commandLineService;
    }

    public bool IsWindowAvailable() => _timerWindow != null;
        
    public bool IsWindowVisible() => _timerWindow is {IsVisible: true};

    public void ShowWindow()
    {
        if (!_commandLineService.IsTimerNdi)
        {
            _timerWindow?.Show();
        }
    }

    public void RelocateWindow()
    {
        var monitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
        if (monitor != null)
        {
            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Relocating timer window to: {MonitorName}", monitor.FriendlyName);
            }

            RelocateWindow(_timerWindow, monitor);
        }
    }

    public void OpenWindowInMonitor()
    {
        var targetMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
        if (targetMonitor != null)
        {
            _timerWindow ??= new TimerOutputWindow(_optionsService, _dateTimeService, _commandLineService);

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
        _timerWindow ??= new TimerOutputWindow(_optionsService, _dateTimeService, _commandLineService);

        ConfigureForWindowedOperation();

        _timerWindow.Show();
        
        _timerWindow.AdjustWindowPositionAndSize();

        if (_commandLineService.IsTimerNdi)
        {
            _timerWindow.Hide();
        }
    }

    public void HideWindow()
    {
        _timerWindow?.Hide();
    }

    public void Close()
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Closing timer window");
        }

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

        if (!_commandLineService.IsTimerNdi)
        {
            _timerWindow.MinHeight = 300;
            _timerWindow.MinWidth = 400;

            _timerWindow.ResizeMode = ResizeMode.CanResize;
            _timerWindow.ShowInTaskbar = true;
            _timerWindow.WindowStyle = WindowStyle.None;
        }
    }
}