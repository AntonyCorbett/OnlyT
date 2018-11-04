namespace OnlyT.ViewModel
{
    // ReSharper disable CatchAllClause
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;
    using AutoUpdates;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using Messages;
    using Models;
    using Serilog;
    using Services.Bell;
    using Services.CountdownTimer;
    using Services.Monitors;
    using Services.Options;
    using Utils;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SettingsPageViewModel : ViewModelBase, IPage
    {
        private readonly MonitorItem[] _monitors;
        private readonly LanguageItem[] _languages;
        private readonly OperatingModeItem[] _operatingModes;
        private readonly AutoMeetingTime[] _autoMeetingTimes;
        private readonly IOptionsService _optionsService;
        private readonly IMonitorsService _monitorsService;
        private readonly IBellService _bellService;
        private readonly ICountdownTimerTriggerService _countdownTimerService;
        private readonly ClockHourFormatItem[] _clockHourFormats;
        private readonly AdaptiveModeItem[] _adaptiveModes;
        private readonly FullScreenClockModeItem[] _timeOfDayModes;
        private readonly WebClockPortItem[] _ports;
        private readonly PersistDurationItem[] _persistDurationItems;

        public SettingsPageViewModel(
           IMonitorsService monitorsService,
           IBellService bellService,
           IOptionsService optionsService,
           ICountdownTimerTriggerService countdownTimerService)
        {
            // subscriptions...
            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
            Messenger.Default.Register<BellStatusChangedMessage>(this, OnBellChanged);

            _optionsService = optionsService;
            _monitorsService = monitorsService;
            _bellService = bellService;
            _countdownTimerService = countdownTimerService;

            _monitors = GetSystemMonitors().ToArray();
            _languages = GetSupportedLanguages();
            _operatingModes = GetOperatingModes().ToArray();
            _autoMeetingTimes = GetAutoMeetingTimes().ToArray();
            _clockHourFormats = GetClockHourFormats().ToArray();
            _adaptiveModes = GetAdaptiveModes().ToArray();
            _timeOfDayModes = GetTimeOfDayModes().ToArray();
            _ports = GetPorts().ToArray();
            _persistDurationItems = optionsService.Options.GetPersistDurationItems().ToArray();
            
            // commands...
            NavigateOperatorCommand = new RelayCommand(NavigateOperatorPage);
            TestBellCommand = new RelayCommand(TestBell, IsNotPlayingBell);
            OpenPortCommand = new RelayCommand(ReserveAndOpenPort);
            WebClockUrlLinkCommand = new RelayCommand(OpenWebClockLink);
        }

        public static string PageName => "SettingsPage";

        public BitmapSource ElevatedShield => NativeMethods.GetElevatedShieldBitmap();

        public IEnumerable<MonitorItem> Monitors => _monitors;

        public string MonitorId
        {
            get => _optionsService.Options.TimerMonitorId;
            set
            {
                if (_optionsService.Options.TimerMonitorId != value)
                {
                    _optionsService.Options.TimerMonitorId = value;
                    RaisePropertyChanged();
                    Messenger.Default.Send(new TimerMonitorChangedMessage());
                }
            }
        }

        public string CountdownMonitorId
        {
            get => _optionsService.Options.CountdownMonitorId;
            set
            {
                if (_optionsService.Options.CountdownMonitorId != value)
                {
                    _optionsService.Options.CountdownMonitorId = value;
                    RaisePropertyChanged();
                    Messenger.Default.Send(new CountdownMonitorChangedMessage());
                }
            }
        }

        public IEnumerable<LanguageItem> Languages => _languages;

        public string LanguageId
        {
            get => _optionsService.Options.Culture;
            set
            {
                if (_optionsService.Options.Culture != value)
                {
                    _optionsService.Options.Culture = value;
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ShouldEnableShowSeconds));
                    Messenger.Default.Send(new ClockHourFormatChangedMessage());
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new ClockHourFormatChangedMessage());
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new OperatingModeChangedMessage());
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(WebClockUrl));
                    RaisePropertyChanged(nameof(WebClockQrCode));

                    Messenger.Default.Send(new HttpServerChangedMessage());
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new AutoMeetingChangedMessage());
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new AutoMeetingChangedMessage());
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new ShowCircuitVisitToggleChangedMessage());
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new CountdownFrameChangedMessage());
                }
            }
        }

        public bool ShowTimerFrame
        {
            get => _optionsService.Options.TimerFrame;
            set
            {
                if (_optionsService.Options.TimerFrame != value)
                {
                    _optionsService.Options.TimerFrame = value;
                    RaisePropertyChanged();
                    Messenger.Default.Send(new TimerFrameChangedMessage());
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new AlwaysOnTopChangedMessage());
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new ShowTimeOfDayUnderTimerChangedMessage());
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new AnalogueClockWidthChangedMessage());
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new HttpServerChangedMessage());
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new HttpServerChangedMessage());
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
                    RaisePropertyChanged();
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
                    RaisePropertyChanged();
                    Messenger.Default.Send(new MousePointerInTimerDisplayChangedMessage());
                }
            }
        }

        public string ApiCode
        {
            get => _optionsService.Options.ApiCode;
            set
            {
                var val = value.Trim();
                if (_optionsService.Options.ApiCode == null || !_optionsService.Options.ApiCode.Equals(val))
                {
                    _optionsService.Options.ApiCode = val;
                    RaisePropertyChanged();
                }
            }
        }

        public BitmapImage WebClockQrCode
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
                string ipAddress = LocalIpAddress.GetLocalIp4Address();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    return $"http://{ipAddress}:{Port}/index";
                }

                return "Web clock not available";
            }
        }

        public string MobileIpAddress
        {
            get
            {
                string ipAddress = LocalIpAddress.GetLocalIp4Address();
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    return ipAddress;
                }

                return "not available";
            }
        }

        public string AppVersionStr => string.Format(Properties.Resources.APP_VER, VersionDetection.GetCurrentVersionString());

        public RelayCommand NavigateOperatorCommand { get; set; }

        public RelayCommand TestBellCommand { get; set; }

        public RelayCommand OpenPortCommand { get; set; }

        public RelayCommand WebClockUrlLinkCommand { get; set; }

        public void Activated(object state)
        {
            // may be changed on operator page...
            RaisePropertyChanged(nameof(IsCircuitVisit));
        }

        private void OnShutDown(ShutDownMessage obj)
        {
            Save();
        }

        private void Save()
        {
            _optionsService.Save();
        }

        private void OpenWebClockLink()
        {
            System.Diagnostics.Process.Start(WebClockUrl);
        }

        private void ReserveAndOpenPort()
        {
            try
            {
                Log.Logger.Information($"Attempting to reserve and open port: {Port}");
                
                int rv = FirewallPortsClient.ReserveAndOpenPort(Port);
                if (rv != 0)
                {
                    Log.Logger.Warning($"Return value from reserve and open port = {rv}");
                }
                else
                {
                    Log.Logger.Information($"Success reserving and opening port: {Port}");
                }
                
                Messenger.Default.Send(new HttpServerChangedMessage());
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not reserve port");
            }
        }

        private IEnumerable<FullScreenClockModeItem> GetTimeOfDayModes()
        {
            return new List<FullScreenClockModeItem>
            {
                new FullScreenClockModeItem { Mode = FullScreenClockMode.Analogue, Name = Properties.Resources.FULL_SCREEN_ANALOGUE },
                new FullScreenClockModeItem { Mode = FullScreenClockMode.Digital, Name = Properties.Resources.FULL_SCREEN_DIGITAL },
                new FullScreenClockModeItem { Mode = FullScreenClockMode.AnalogueAndDigital, Name = Properties.Resources.FULL_SCREEN_BOTH }
            };
        }

        private IEnumerable<AdaptiveModeItem> GetAdaptiveModes()
        {
            return new List<AdaptiveModeItem>
            {
                new AdaptiveModeItem { Mode = AdaptiveMode.None, Name = Properties.Resources.ADAPTIVE_MODE_NONE },
                new AdaptiveModeItem { Mode = AdaptiveMode.OneWay, Name = Properties.Resources.ADAPTIVE_MODE_ONE_WAY },
                new AdaptiveModeItem { Mode = AdaptiveMode.TwoWay, Name = Properties.Resources.ADAPTIVE_MODE_TWO_WAY }
            };
        }

        private IEnumerable<WebClockPortItem> GetPorts()
        {
            var result = new List<WebClockPortItem>();

            for (int n = Options.DefaultPort; n <= Options.DefaultPort + Options.MaxPossiblePorts; ++n)
            {
                result.Add(new WebClockPortItem { Port = n });
            }

            return result;
        }

        private IEnumerable<ClockHourFormatItem> GetClockHourFormats()
        {
            var cultureUsesAmPm = !string.IsNullOrEmpty(DateTime.Now.ToString("tt", CultureInfo.CurrentUICulture));

            var result = new List<ClockHourFormatItem>
            {
                new ClockHourFormatItem
                {
                    Name = Properties.Resources.CLOCK_FORMAT_12, Format = ClockHourFormat.Format12
                },
                new ClockHourFormatItem
                {
                    Name = Properties.Resources.CLOCK_FORMAT_12Z, Format = ClockHourFormat.Format12LeadingZero
                }
            };
            
            if (cultureUsesAmPm)
            {
                result.Add(new ClockHourFormatItem
                    { Name = Properties.Resources.CLOCK_FORMAT_12AMPM, Format = ClockHourFormat.Format12AMPM });

                result.Add(new ClockHourFormatItem
                    { Name = Properties.Resources.CLOCK_FORMAT_12ZAMPM, Format = ClockHourFormat.Format12LeadingZeroAMPM });
            }

            result.Add(new ClockHourFormatItem
                { Name = Properties.Resources.CLOCK_FORMAT_24, Format = ClockHourFormat.Format24 });

            result.Add(new ClockHourFormatItem
                { Name = Properties.Resources.CLOCK_FORMAT_24Z, Format = ClockHourFormat.Format24LeadingZero });

            return result;
        }

        private void OnBellChanged(BellStatusChangedMessage message)
        {
            TestBellCommand.RaiseCanExecuteChanged();
        }

        private bool IsNotPlayingBell()
        {
            return !_bellService.IsPlaying;
        }

        private void TestBell()
        {
            _bellService.Play(_optionsService.Options.BellVolumePercent);
        }

        private IEnumerable<AutoMeetingTime> GetAutoMeetingTimes()
        {
            return new List<AutoMeetingTime>
            {
                new AutoMeetingTime { Name = Properties.Resources.MIDWEEK, Id = MidWeekOrWeekend.MidWeek },
                new AutoMeetingTime { Name = Properties.Resources.WEEKEND, Id = MidWeekOrWeekend.Weekend }
            };
        }

        private IEnumerable<OperatingModeItem> GetOperatingModes()
        {
            return new List<OperatingModeItem>
            {
                new OperatingModeItem { Name = Properties.Resources.OP_MODE_MANUAL, Mode = OperatingMode.Manual },
                new OperatingModeItem { Name = Properties.Resources.OP_MODE_FILE, Mode = OperatingMode.ScheduleFile },
                new OperatingModeItem { Name = Properties.Resources.OP_MODE_AUTO, Mode = OperatingMode.Automatic }
            };
        }

        private LanguageItem[] GetSupportedLanguages()
        {
            var result = new List<LanguageItem>();

            var subFolders = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory);

            foreach (var folder in subFolders)
            {
                if (!string.IsNullOrEmpty(folder))
                {
                    try
                    {
                        var c = new CultureInfo(Path.GetFileNameWithoutExtension(folder));
                        result.Add(new LanguageItem
                        {
                            LanguageId = c.Name,
                            LanguageName = c.EnglishName
                        });
                    }
                    catch (CultureNotFoundException)
                    {
                        // expected
                    }
                }
            }

            // the native language
            {
                var c = new CultureInfo(Path.GetFileNameWithoutExtension("en-GB"));
                result.Add(new LanguageItem
                {
                    LanguageId = c.Name,
                    LanguageName = c.EnglishName
                });
            }

            result.Sort((x, y) => string.Compare(x.LanguageName, y.LanguageName, StringComparison.Ordinal));

            return result.ToArray();
        }

        private IEnumerable<MonitorItem> GetSystemMonitors()
        {
            var result = new List<MonitorItem>
            {
                // empty (i.e. no timer monitor)
                new MonitorItem
                {
                    MonitorName = Properties.Resources.MONITOR_NONE,
                    FriendlyName = Properties.Resources.MONITOR_NONE
                } 
            };  

            result.AddRange(_monitorsService.GetSystemMonitors());
            return result;
        }

        private void NavigateOperatorPage()
        {
            Save();
            Messenger.Default.Send(new NavigateMessage(PageName, OperatorPageViewModel.PageName, null));
        }
    }
}
