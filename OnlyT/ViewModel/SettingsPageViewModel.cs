// ReSharper disable CatchAllClause
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using OnlyT.AutoUpdates;
using OnlyT.Models;
using OnlyT.Services.Bell;
using OnlyT.Services.CountdownTimer;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Windows.Media.Imaging;
using OnlyT.Common.Services.DateTime;
using OnlyT.CountdownTimer;
using OnlyT.Properties;
using OnlyT.Services.Snackbar;
using Serilog;
using Serilog.Events;
using System.IO;

namespace OnlyT.ViewModel
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingsPageViewModel : ObservableObject, IPage
    {
        private readonly MonitorItem[] _monitors;
        private readonly LanguageItem[] _languages;
        private readonly OperatingModeItem[] _operatingModes;
        private readonly OnScreenLocationItem[] _screenLocationItems;
        private readonly CountdownDurationItem[] _countdownDurationItems;
        private readonly CountdownElementsToShowItem[] _countdownElementsToShowItems;
        private readonly AutoMeetingTime[] _autoMeetingTimes;
        private readonly IOptionsService _optionsService;
        private readonly ISnackbarService _snackbarService;
        private readonly IMonitorsService _monitorsService;
        private readonly IBellService _bellService;
        private readonly ICountdownTimerTriggerService _countdownTimerService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ClockHourFormatItem[] _clockHourFormats;
        private readonly AdaptiveModeItem[] _adaptiveModes;
        private readonly FullScreenClockModeItem[] _timeOfDayModes;
        private readonly WebClockPortItem[] _ports;
        private readonly PersistDurationItem[] _persistDurationItems;
        private readonly LoggingLevel[] _loggingLevels;

        public SettingsPageViewModel(
           IMonitorsService monitorsService,
           IBellService bellService,
           IOptionsService optionsService,
           ISnackbarService snackbarService,
           ICountdownTimerTriggerService countdownTimerService,
           IDateTimeService dateTimeService)
        {
            // subscriptions...
            WeakReferenceMessenger.Default.Register<ShutDownMessage>(this, OnShutDown);
            WeakReferenceMessenger.Default.Register<BellStatusChangedMessage>(this, OnBellChanged);

            _optionsService = optionsService;
            _snackbarService = snackbarService;
            _monitorsService = monitorsService;
            _bellService = bellService;
            _countdownTimerService = countdownTimerService;
            _dateTimeService = dateTimeService;

            _monitors = GetSystemMonitors();
            _languages = GetSupportedLanguages();
            _operatingModes = GetOperatingModes();
            _screenLocationItems = GetScreenLocationItems();
            _countdownDurationItems = GetCountdownDurationItems();
            _countdownElementsToShowItems = GetCountdownElementsToShowItems();
            _autoMeetingTimes = GetAutoMeetingTimes();
            _clockHourFormats = GetClockHourFormats();
            _adaptiveModes = GetAdaptiveModes();
            _timeOfDayModes = GetTimeOfDayModes();
            _ports = GetPorts().ToArray();
            _persistDurationItems = Options.GetPersistDurationItems();
            _loggingLevels = GetLoggingLevels();

            // commands...
            NavigateOperatorCommand = new RelayCommand(NavigateOperatorPage);
            TestBellCommand = new RelayCommand(TestBell, IsNotPlayingBell);
            OpenPortCommand = new RelayCommand(ReserveAndOpenPort);
            WebClockUrlLinkCommand = new RelayCommand(OpenWebClockLink);
        }

        public static string PageName => "SettingsPage";

        public static BitmapSource ElevatedShield => NativeMethods.GetElevatedShieldBitmap();

        public IEnumerable<MonitorItem> Monitors => _monitors;

        public string? MonitorId
        {
            get => _optionsService.Options.TimerMonitorId;
            set
            {
                if (_optionsService.Options.TimerMonitorId != value)
                {
                    var change = GetChangeInMonitor(_optionsService.Options.TimerMonitorId, value);

                    _optionsService.Options.TimerMonitorId = value;
                    OnPropertyChanged();

                    WeakReferenceMessenger.Default.Send(new TimerMonitorChangedMessage(change));
                }
            }
        }

        public bool IsTimerMonitorViaCommandLine => _optionsService.IsTimerMonitorSetByCommandLine;

        public bool AllowMainMonitorSelection => !IsTimerMonitorViaCommandLine && !MainMonitorIsWindowed;

        public bool IsCountdownMonitorViaCommandLine => _optionsService.IsCountdownMonitorSetByCommandLine;

        public bool AllowCountdownMonitorSelection => !IsCountdownMonitorViaCommandLine && !CountdownMonitorIsWindowed;

        public string? CountdownMonitorId
        {
            get => _optionsService.Options.CountdownMonitorId;
            set
            {
                if (_optionsService.Options.CountdownMonitorId != value)
                {
                    var change = GetChangeInMonitor(_optionsService.Options.CountdownMonitorId, value);

                    _optionsService.Options.CountdownMonitorId = value;
                    OnPropertyChanged();

                    WeakReferenceMessenger.Default.Send(new CountdownMonitorChangedMessage(change));
                }
            }
        }

        public IEnumerable<LanguageItem> Languages => _languages;

        public string? LanguageId
        {
            get => _optionsService.Options.Culture;
            set
            {
                if (_optionsService.Options.Culture != value)
                {
                    _optionsService.Options.Culture = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<ClockHourFormatItem> ClockHourFormats => _clockHourFormats;

        public ClockHourFormat ClockHourFormat
        {
            get => _optionsService.Options.ClockHourFormat;
            set
            {
                if (_optionsService.Options.ClockHourFormat != value)
                {
                    _optionsService.Options.ClockHourFormat = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ShouldEnableShowSeconds));
                    WeakReferenceMessenger.Default.Send(new ClockHourFormatChangedMessage());
                }
            }
        }

        public bool ShouldEnableShowSeconds =>
            ClockHourFormat != ClockHourFormat.Format12AMPM &&
            ClockHourFormat != ClockHourFormat.Format12LeadingZeroAMPM;

        public bool ShowDigitalSeconds
        {
            get => _optionsService.Options.ShowDigitalSeconds;
            set
            {
                if (_optionsService.Options.ShowDigitalSeconds != value)
                {
                    _optionsService.Options.ShowDigitalSeconds = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new ClockHourFormatChangedMessage());
                }
            }
        }

        public IEnumerable<CountdownElementsToShowItem> CountdownElementsToShowItems => _countdownElementsToShowItems;

        public ElementsToShow CountdownElementsToShow
        {
            get => _optionsService.Options.CountdownElementsToShow;
            set
            {
                if (_optionsService.Options.CountdownElementsToShow != value)
                {
                    _optionsService.Options.CountdownElementsToShow = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new CountdownElementsChangedMessage());
                }
            }
        }

        public IEnumerable<CountdownDurationItem> CountdownDurationItems => _countdownDurationItems;

        public int CountdownDurationMins
        {
            get => _optionsService.Options.CountdownDurationMins;
            set
            {
                if (_optionsService.Options.CountdownDurationMins != value)
                {
                    _optionsService.Options.CountdownDurationMins = value;
                    OnPropertyChanged();
                    _countdownTimerService.UpdateTriggerPeriods();
                }
            }
        }

        public bool IsCountdownWindowTransparent
        {
            get => _optionsService.Options.IsCountdownWindowTransparent;
            set
            {
                if (_optionsService.Options.IsCountdownWindowTransparent != value)
                {
                    _optionsService.Options.IsCountdownWindowTransparent = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new CountdownWindowTransparencyChangedMessage());
                }
            }
        }

        public IEnumerable<OnScreenLocationItem> ScreenLocationItems => _screenLocationItems;

        public ScreenLocation CountdownScreenLocation
        {
            get => _optionsService.Options.CountdownScreenLocation;
            set
            {
                if (_optionsService.Options.CountdownScreenLocation != value)
                {
                    _optionsService.Options.CountdownScreenLocation = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new CountdownZoomOrPositionChangedMessage());
                }
            }
        }

        public IEnumerable<OperatingModeItem> OperatingModes => _operatingModes;

        public OperatingMode OperatingMode
        {
            get => _optionsService.Options.OperatingMode;
            set
            {
                if (_optionsService.Options.OperatingMode != value)
                {
                    _optionsService.Options.OperatingMode = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new OperatingModeChangedMessage());
                }
            }
        }

        public IEnumerable<FullScreenClockModeItem> TimeOfDayModes => _timeOfDayModes;

        public FullScreenClockMode TimeOfDayMode
        {
            get => _optionsService.Options.FullScreenClockMode;
            set
            {
                if (_optionsService.Options.FullScreenClockMode != value)
                {
                    _optionsService.Options.FullScreenClockMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<AdaptiveModeItem> AdaptiveModes => _adaptiveModes;

        public AdaptiveMode MidWeekAdaptiveMode
        {
            get => _optionsService.Options.MidWeekAdaptiveMode;
            set
            {
                if (_optionsService.Options.MidWeekAdaptiveMode != value)
                {
                    _optionsService.Options.MidWeekAdaptiveMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<PersistDurationItem> PersistDurationItems => _persistDurationItems;

        public int PersistDurationSecs
        {
            get => _optionsService.Options.PersistDurationSecs;
            set
            {
                if (_optionsService.Options.PersistDurationSecs != value)
                {
                    _optionsService.Options.PersistDurationSecs = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<WebClockPortItem> Ports => _ports;

        public int Port
        {
            get => _optionsService.Options.HttpServerPort;
            set
            {
                if (_optionsService.Options.HttpServerPort != value)
                {
                    _optionsService.Options.HttpServerPort = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(WebClockUrl));
                    OnPropertyChanged(nameof(WebClockQrCode));

                    WeakReferenceMessenger.Default.Send(new HttpServerChangedMessage());
                }
            }
        }

        public AdaptiveMode WeekendAdaptiveMode
        {
            get => _optionsService.Options.WeekendAdaptiveMode;
            set
            {
                if (_optionsService.Options.WeekendAdaptiveMode != value)
                {
                    _optionsService.Options.WeekendAdaptiveMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<AutoMeetingTime> AutoMeetingTimes => _autoMeetingTimes;

        public MidWeekOrWeekend MidWeekOrWeekend
        {
            get => _optionsService.Options.MidWeekOrWeekend;
            set
            {
                if (_optionsService.Options.MidWeekOrWeekend != value)
                {
                    _optionsService.Options.MidWeekOrWeekend = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new AutoMeetingChangedMessage());
                }
            }
        }

        public bool JwLibraryCompatibilityMode
        {
            get => _optionsService.Options.JwLibraryCompatibilityMode;
            set
            {
                if (_optionsService.Options.JwLibraryCompatibilityMode != value)
                {
                    _optionsService.Options.JwLibraryCompatibilityMode = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool PersistStudentTime
        {
            get => _optionsService.Options.PersistStudentTime;
            set
            {
                if (_optionsService.Options.PersistStudentTime != value)
                {
                    _optionsService.Options.PersistStudentTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCircuitVisit
        {
            get => _optionsService.Options.IsCircuitVisit;
            set
            {
                if (_optionsService.Options.IsCircuitVisit != value)
                {
                    _optionsService.Options.IsCircuitVisit = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new AutoMeetingChangedMessage());
                }
            }
        }

        public bool MainMonitorIsWindowed
        {
            get => _optionsService.Options.MainMonitorIsWindowed;
            set
            {
                if (_optionsService.Options.MainMonitorIsWindowed != value)
                {
                    var change = GetChangeInMonitor(_optionsService.Options.TimerMonitorId, value);

                    _optionsService.Options.MainMonitorIsWindowed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AllowMainMonitorSelection));

                    WeakReferenceMessenger.Default.Send(new TimerMonitorChangedMessage(change));
                }
            }
        }

        public bool CountdownMonitorIsWindowed
        {
            get => _optionsService.Options.CountdownMonitorIsWindowed;
            set
            {
                if (_optionsService.Options.CountdownMonitorIsWindowed != value)
                {
                    var change = GetChangeInMonitor(_optionsService.Options.CountdownMonitorId, value);

                    _optionsService.Options.CountdownMonitorIsWindowed = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AllowCountdownMonitorSelection));

                    WeakReferenceMessenger.Default.Send(new CountdownMonitorChangedMessage(change));
                }
            }
        }

        public bool WeekendIncludesFriday
        {
            get => _optionsService.Options.WeekendIncludesFriday;
            set
            {
                if (_optionsService.Options.WeekendIncludesFriday != value)
                {
                    _optionsService.Options.WeekendIncludesFriday = value;
                    OnPropertyChanged();

                    // may need to change the talk schedule (i.e. if today is Friday)...
                    MidWeekOrWeekend = _optionsService.IsNowWeekend()
                        ? MidWeekOrWeekend.Weekend
                        : MidWeekOrWeekend.MidWeek;
                }
            }
        }

        public bool ShowCircuitVisitToggle
        {
            get => _optionsService.Options.ShowCircuitVisitToggle;
            set
            {
                if (_optionsService.Options.ShowCircuitVisitToggle != value)
                {
                    _optionsService.Options.ShowCircuitVisitToggle = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new ShowCircuitVisitToggleChangedMessage());
                }
            }
        }

        public bool ShowCountdownFrame
        {
            get => _optionsService.Options.CountdownFrame;
            set
            {
                if (_optionsService.Options.CountdownFrame != value)
                {
                    _optionsService.Options.CountdownFrame = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new CountdownFrameChangedMessage());
                }
            }
        }

        public bool ShowClockFrame
        {
            get => _optionsService.Options.TimerFrame;
            set
            {
                if (_optionsService.Options.TimerFrame != value)
                {
                    _optionsService.Options.TimerFrame = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new TimerFrameChangedMessage());
                }
            }
        }

        public bool ShowClockTimerFrame
        {
            get => _optionsService.Options.ClockTimerFrame;
            set
            {
                if (_optionsService.Options.ClockTimerFrame != value)
                {
                    _optionsService.Options.ClockTimerFrame = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new TimerFrameChangedMessage());
                }
            }
        }

        public bool ShowTimerFrame
        {
            get => _optionsService.Options.ClockTimerFrame;
            set
            {
                if (_optionsService.Options.ClockTimerFrame != value)
                {
                    _optionsService.Options.ClockTimerFrame = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new TimerFrameChangedMessage());
                }
            }
        }

        public string MeetingStartTimesAsText
        {
            get => _optionsService.Options.MeetingStartTimes.AsText();
            set
            {
                var times = _optionsService.Options.MeetingStartTimes.AsText();
                if (!times.Equals(value))
                {
                    _optionsService.Options.MeetingStartTimes.FromText(value);
                    OnPropertyChanged();
                    _countdownTimerService.UpdateTriggerPeriods();
                }
            }
        }

        public bool AllowCountUpToggle
        {
            get => _optionsService.Options.AllowCountUpToggle;
            set
            {
                if (_optionsService.Options.AllowCountUpToggle != value)
                {
                    _optionsService.Options.AllowCountUpToggle = value;
                    OnPropertyChanged();
                }
            }
        }

        public IEnumerable<LoggingLevel> LoggingLevels => _loggingLevels;

        public LogEventLevel LogEventLevel
        {
            get => _optionsService.Options.LogEventLevel;
            set
            {
                if (_optionsService.Options.LogEventLevel != value)
                {
                    _optionsService.Options.LogEventLevel = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new LogLevelChangedMessage());
                }
            }
        }

        public bool AlwaysOnTop
        {
            get => _optionsService.Options.AlwaysOnTop;
            set
            {
                if (_optionsService.Options.AlwaysOnTop != value)
                {
                    _optionsService.Options.AlwaysOnTop = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new AlwaysOnTopChangedMessage());
                }
            }
        }

        public bool ShouldGenerateReports
        {
            get => _optionsService.Options.GenerateTimingReports;
            set
            {
                if (_optionsService.Options.GenerateTimingReports != value)
                {
                    _optionsService.Options.GenerateTimingReports = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowBackgroundOnTimer
        {
            get => _optionsService.Options.ShowBackgroundOnTimer;
            set
            {
                if (_optionsService.Options.ShowBackgroundOnTimer != value)
                {
                    _optionsService.Options.ShowBackgroundOnTimer = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new TimerFrameChangedMessage());
                }
            }
        }

        public bool ShowBackgroundOnClock
        {
            get => _optionsService.Options.ShowBackgroundOnClock;
            set
            {
                if (_optionsService.Options.ShowBackgroundOnClock != value)
                {
                    _optionsService.Options.ShowBackgroundOnClock = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new TimerFrameChangedMessage());
                }
            }
        }

        public bool ShowTimeOfDayUnderTimer
        {
            get => _optionsService.Options.ShowTimeOfDayUnderTimer;
            set
            {
                if (_optionsService.Options.ShowTimeOfDayUnderTimer != value)
                {
                    _optionsService.Options.ShowTimeOfDayUnderTimer = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new ShowTimeOfDayUnderTimerChangedMessage());
                }
            }
        }

        public bool ShowDurationSector
        {
            get => _optionsService.Options.ShowDurationSector;
            set
            {
                if (_optionsService.Options.ShowDurationSector != value)
                {
                    _optionsService.Options.ShowDurationSector = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool CountUp
        {
            get => _optionsService.Options.CountUp;
            set
            {
                if (_optionsService.Options.CountUp != value)
                {
                    _optionsService.Options.CountUp = value;
                    OnPropertyChanged();
                }
            }
        }

        public int AnalogueClockWidthPercent
        {
            get => _optionsService.Options.AnalogueClockWidthPercent;
            set
            {
                if (_optionsService.Options.AnalogueClockWidthPercent != value)
                {
                    _optionsService.Options.AnalogueClockWidthPercent = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new AnalogueClockWidthChangedMessage());
                }
            }
        }

        public int CountdownZoomPercent
        {
            get => _optionsService.Options.CountdownZoomPercent;
            set
            {
                if (_optionsService.Options.CountdownZoomPercent != value)
                {
                    _optionsService.Options.CountdownZoomPercent = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new CountdownZoomOrPositionChangedMessage());
                }
            }
        }

        public bool IsBellEnabled
        {
            get => _optionsService.Options.IsBellEnabled;
            set
            {
                if (_optionsService.Options.IsBellEnabled != value)
                {
                    _optionsService.Options.IsBellEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool AutoBell
        {
            get => _optionsService.Options.AutoBell;
            set
            {
                if (_optionsService.Options.AutoBell != value)
                {
                    _optionsService.Options.AutoBell = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new AutoBellSettingChangedMessage());
                }
            }
        }

        public int BellVolumePercent
        {
            get => _optionsService.Options.BellVolumePercent;
            set
            {
                if (_optionsService.Options.BellVolumePercent != value)
                {
                    _optionsService.Options.BellVolumePercent = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsWebClockEnabled
        {
            get => _optionsService.Options.IsWebClockEnabled;
            set
            {
                if (_optionsService.Options.IsWebClockEnabled != value)
                {
                    _optionsService.Options.IsWebClockEnabled = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new HttpServerChangedMessage());
                }
            }
        }

        public bool IsApiEnabled
        {
            get => _optionsService.Options.IsApiEnabled;
            set
            {
                if (_optionsService.Options.IsApiEnabled != value)
                {
                    _optionsService.Options.IsApiEnabled = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new HttpServerChangedMessage());
                }
            }
        }

        public bool IsApiThrottled
        {
            get => _optionsService.Options.IsApiThrottled;
            set
            {
                if (_optionsService.Options.IsApiThrottled != value)
                {
                    _optionsService.Options.IsApiThrottled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowMousePointerInTimerDisplay
        {
            get => _optionsService.Options.ShowMousePointerInTimerDisplay;
            set
            {
                if (_optionsService.Options.ShowMousePointerInTimerDisplay != value)
                {
                    _optionsService.Options.ShowMousePointerInTimerDisplay = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new MousePointerInTimerDisplayChangedMessage());
                }
            }
        }

        public bool ClockIsFlat
        {
            get => _optionsService.Options.ClockIsFlat;
            set
            {
                if (_optionsService.Options.ClockIsFlat != value)
                {
                    _optionsService.Options.ClockIsFlat = value;
                    OnPropertyChanged();
                    WeakReferenceMessenger.Default.Send(new ClockIsFlatChangedMessage());
                }
            }
        }

        public string? ApiCode
        {
            get => _optionsService.Options.ApiCode;
            set
            {
                var val = value?.Trim();
                if (_optionsService.Options.ApiCode != val)
                {
                    _optionsService.Options.ApiCode = val;
                    OnPropertyChanged();
                }
            }
        }

        public BitmapImage? WebClockQrCode
        {
            get
            {
                var url = WebClockUrl;
                if (!string.IsNullOrEmpty(url) && url.StartsWith("http"))
                {
                    return QRCodeGeneration.CreateQRCode(WebClockUrl);
                }

                return null;
            }
        }

        public string WebClockUrl
        {
            get
            {
                var ipAddress = LocalIpAddress.GetLocalIp4Address();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    return $"http://{ipAddress}:{Port}/index";
                }

                return "Web clock not available";
            }
        }

        public static string MobileIpAddress
        {
            get
            {
                var ipAddress = LocalIpAddress.GetLocalIp4Address();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    return ipAddress;
                }

                return "not available";
            }
        }

        public static string AppVersionStr => string.Format(Properties.Resources.APP_VER, VersionDetection.GetCurrentVersionString());

        public RelayCommand NavigateOperatorCommand { get; set; }

        public RelayCommand TestBellCommand { get; set; }

        public RelayCommand OpenPortCommand { get; set; }

        public RelayCommand WebClockUrlLinkCommand { get; set; }

        public void Activated(object? state)
        {
            // may be changed on operator page...
            OnPropertyChanged(nameof(IsCircuitVisit));
        }

        private void OnShutDown(object recipient, ShutDownMessage obj)
        {
            Save();
        }

        private void Save()
        {
            _optionsService.Save();
        }

        private void OpenWebClockLink()
        {
            var psi = new ProcessStartInfo
            {
                FileName = WebClockUrl,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private void ReserveAndOpenPort()
        {
            try
            {
                Log.Logger.Information($"Attempting to reserve and open port: {Port}");

                var rv = FirewallPortsClient.ReserveAndOpenPort(Port);
                if (rv != 0)
                {
                    Log.Logger.Warning($"Return value from reserve and open port = {rv}");

                    _snackbarService.EnqueueWithOk(Properties.Resources.PORT_OPEN_FAILED);
                }
                else
                {
                    Log.Logger.Information($"Success reserving and opening port: {Port}");

                    _snackbarService.EnqueueWithOk(Properties.Resources.PORT_OPENED);
                }

                WeakReferenceMessenger.Default.Send(new HttpServerChangedMessage());
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not reserve port");
                _snackbarService.EnqueueWithOk(Properties.Resources.PORT_OPEN_FAILED);
            }
        }

        private static FullScreenClockModeItem[] GetTimeOfDayModes()
        {
            return new[]
            {
                new FullScreenClockModeItem(FullScreenClockMode.Analogue, Properties.Resources.FULL_SCREEN_ANALOGUE),
                new FullScreenClockModeItem(FullScreenClockMode.Digital, Properties.Resources.FULL_SCREEN_DIGITAL),
                new FullScreenClockModeItem(FullScreenClockMode.AnalogueAndDigital, Properties.Resources.FULL_SCREEN_BOTH)
            };
        }

        private static AdaptiveModeItem[] GetAdaptiveModes()
        {
            return new[]
            {
                new AdaptiveModeItem(AdaptiveMode.None, Properties.Resources.ADAPTIVE_MODE_NONE),
                new AdaptiveModeItem(AdaptiveMode.OneWay, Properties.Resources.ADAPTIVE_MODE_ONE_WAY),
                new AdaptiveModeItem(AdaptiveMode.TwoWay, Properties.Resources.ADAPTIVE_MODE_TWO_WAY)
            };
        }

        private static IEnumerable<WebClockPortItem> GetPorts()
        {
            var result = new List<WebClockPortItem>();

            for (int n = Options.DefaultPort; n <= Options.DefaultPort + Options.MaxPossiblePorts; ++n)
            {
                result.Add(new WebClockPortItem { Port = n });
            }

            return result;
        }

        private ClockHourFormatItem[] GetClockHourFormats()
        {
            var cultureUsesAmPm = !string.IsNullOrEmpty(_dateTimeService.Now().ToString("tt", CultureInfo.CurrentUICulture));

            var result = new List<ClockHourFormatItem>
            {
                new(Properties.Resources.CLOCK_FORMAT_12, ClockHourFormat.Format12),
                new(Properties.Resources.CLOCK_FORMAT_12Z, ClockHourFormat.Format12LeadingZero)
            };

            if (cultureUsesAmPm)
            {
                result.Add(new ClockHourFormatItem(Properties.Resources.CLOCK_FORMAT_12AMPM, ClockHourFormat.Format12AMPM));
                result.Add(new ClockHourFormatItem(Properties.Resources.CLOCK_FORMAT_12ZAMPM, ClockHourFormat.Format12LeadingZeroAMPM));
            }

            result.Add(new ClockHourFormatItem(Properties.Resources.CLOCK_FORMAT_24, ClockHourFormat.Format24));
            result.Add(new ClockHourFormatItem(Properties.Resources.CLOCK_FORMAT_24Z, ClockHourFormat.Format24LeadingZero));

            return result.ToArray();
        }

        private void OnBellChanged(object recipient, BellStatusChangedMessage message)
        {
            TestBellCommand.NotifyCanExecuteChanged();
        }

        private bool IsNotPlayingBell()
        {
            return !_bellService.IsPlaying;
        }

        private void TestBell()
        {
            _bellService.Play(_optionsService.Options.BellVolumePercent);
        }

        private static AutoMeetingTime[] GetAutoMeetingTimes()
        {
            return new[]
            {
                new AutoMeetingTime(MidWeekOrWeekend.MidWeek, Properties.Resources.MIDWEEK),
                new AutoMeetingTime(MidWeekOrWeekend.Weekend, Properties.Resources.WEEKEND)
            };
        }

        private static CountdownElementsToShowItem[] GetCountdownElementsToShowItems()
        {
            return new[]
            {
                new CountdownElementsToShowItem(ElementsToShow.DialAndDigital, Properties.Resources.DIAL_AND_DIGITAL),
                new CountdownElementsToShowItem(ElementsToShow.Dial, Properties.Resources.DIAL),
                new CountdownElementsToShowItem(ElementsToShow.Digital, Properties.Resources.DIGITAL)
            };
        }

        private static CountdownDurationItem[] GetCountdownDurationItems()
        {
            return Options.GetCountdownDurationItems();
        }

        private static OnScreenLocationItem[] GetScreenLocationItems()
        {
            return new[]
            {
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_CENTRE, ScreenLocation.Centre),
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_LEFT, ScreenLocation.Left),
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_TOP, ScreenLocation.Top),
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_RIGHT, ScreenLocation.Right),
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_BOTTOM, ScreenLocation.Bottom),
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_TOP_LEFT, ScreenLocation.TopLeft),
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_TOP_RIGHT, ScreenLocation.TopRight),
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_BOTTOM_LEFT, ScreenLocation.BottomLeft),
                new OnScreenLocationItem(Properties.Resources.SCREEN_LOCATION_BOTTOM_RIGHT, ScreenLocation.BottomRight),
            };
        }

        private static OperatingModeItem[] GetOperatingModes()
        {
            return new[]
            {
                new OperatingModeItem(Properties.Resources.OP_MODE_MANUAL, OperatingMode.Manual),
                new OperatingModeItem(Properties.Resources.OP_MODE_FILE, OperatingMode.ScheduleFile),
                new OperatingModeItem(Properties.Resources.OP_MODE_AUTO, OperatingMode.Automatic)
            };
        }

        private static LanguageItem[] GetSupportedLanguages()
        {
            var result = new List<LanguageItem>();

            var subFolders = Directory.GetDirectories(AppContext.BaseDirectory);

            foreach (var folder in subFolders)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    try
                    {
                        var c = new CultureInfo(Path.GetFileNameWithoutExtension(folder));
                        result.Add(new LanguageItem(c.Name, c.EnglishName));
                    }
                    catch (CultureNotFoundException)
                    {
                        // expected
                    }
                }
            }

            // the native language
            var cNative = new CultureInfo(Path.GetFileNameWithoutExtension("en-GB"));
            result.Add(new LanguageItem(cNative.Name, cNative.EnglishName));
        
            result.Sort((x, y) => string.CompareOrdinal(x.LanguageName, y.LanguageName));

            return result.ToArray();
        }

        private MonitorItem[] GetSystemMonitors()
        {
            var result = new List<MonitorItem>
            {
                // empty (i.e. no timer monitor)
                new(null, Properties.Resources.MONITOR_NONE, null, Properties.Resources.MONITOR_NONE)
            };

            result.AddRange(_monitorsService.GetSystemMonitors());
            return result.ToArray();
        }

        private void NavigateOperatorPage()
        {
            Save();
            WeakReferenceMessenger.Default.Send(new NavigateMessage(PageName, OperatorPageViewModel.PageName, null));
        }

        private static LoggingLevel[] GetLoggingLevels()
        {
            var result = new List<LoggingLevel>();

            foreach (LogEventLevel v in Enum.GetValues(typeof(LogEventLevel)))
            {
                result.Add(new LoggingLevel(v.GetDescriptiveName(), v));
            }

            return result.ToArray();
        }

        private static MonitorChangeDescription GetChangeInMonitor(string? monitorId, bool newWindowedMode)
        {
            if (newWindowedMode)
            {
                return string.IsNullOrEmpty(monitorId)
                    ? MonitorChangeDescription.NoneToWindow
                    : MonitorChangeDescription.MonitorToWindow;
            }

            return string.IsNullOrEmpty(monitorId)
                ? MonitorChangeDescription.WindowToNone
                : MonitorChangeDescription.WindowToMonitor;
        }

        private static MonitorChangeDescription GetChangeInMonitor(string? origMonitorId, string? newMonitorId)
        {
            if (string.IsNullOrEmpty(origMonitorId))
            {
                return MonitorChangeDescription.NoneToMonitor;
            }

            if (string.IsNullOrEmpty(newMonitorId))
            {
                return MonitorChangeDescription.MonitorToNone;
            }

            return MonitorChangeDescription.MonitorToMonitor;
        }
    }
}
