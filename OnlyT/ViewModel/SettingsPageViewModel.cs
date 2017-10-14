using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Models;
using OnlyT.Services.Bell;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;
using Serilog;

namespace OnlyT.ViewModel
{
    using System.Configuration;

    public class SettingsPageViewModel : ViewModelBase, IPage
    {
        public static string PageName => "SettingsPage";
        
        private readonly MonitorItem[] _monitors;
        private readonly OperatingModeItem[] _operatingModes;
        private readonly AutoMeetingTime[] _autoMeetingTimes;
        private readonly IOptionsService _optionsService;
        private readonly IMonitorsService _monitorsService;
        private readonly IBellService _bellService;
        private readonly ClockHourFormatItem[] _clockHourFormats;
        private readonly AdaptiveModeItem[] _adaptiveModes;
        private readonly FullScreenClockModeItem[] _timeOfDayModes;
        private readonly WebClockPortItem[] _ports;

        public SettingsPageViewModel(
           IMonitorsService monitorsService,
           IBellService bellService,
           IOptionsService optionsService)
        {
            // subscriptions...
            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
            Messenger.Default.Register<BellStatusChangedMessage>(this, OnBellChanged);

            _optionsService = optionsService;
            _monitorsService = monitorsService;
            _bellService = bellService;

            _monitors = GetSystemMonitors().ToArray();
            _operatingModes = GetOperatingModes().ToArray();
            _autoMeetingTimes = GetAutoMeetingTimes().ToArray();
            _clockHourFormats = GetClockHourFormats().ToArray();
            _adaptiveModes = GetAdaptiveModes().ToArray();
            _timeOfDayModes = GetTimeOfDayModes().ToArray();
            _ports = GetPorts().ToArray();

            // commands...
            NavigateOperatorCommand = new RelayCommand(NavigateOperatorPage);
            TestBellCommand = new RelayCommand(TestBell, IsNotPlayingBell);
            OpenPortCommand = new RelayCommand(ReservePort);
        }

        private void ReservePort()
        {
            try
            {
                int rv = FirewallPortsClient.ReservePort(Port);
                if (rv != 0)
                {
                    Log.Logger.Warning($"Return value from reserve port = {rv}");
                }
                else
                {
                    Log.Logger.Information($"Port reserved: {Port}");
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not reserve port");
            }
        }

        public BitmapSource ElevatedShield => Utils.NativeMethods.GetElevatedShieldBitmap();

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
                new AdaptiveModeItem {Mode = AdaptiveMode.None, Name = Properties.Resources.ADAPTIVE_MODE_NONE},
                new AdaptiveModeItem {Mode = AdaptiveMode.OneWay, Name = Properties.Resources.ADAPTIVE_MODE_ONE_WAY},
                new AdaptiveModeItem {Mode = AdaptiveMode.TwoWay, Name = Properties.Resources.ADAPTIVE_MODE_TWO_WAY}
            };
        }

        private IEnumerable<WebClockPortItem> GetPorts()
        {
            var result = new List<WebClockPortItem>();
            
            for(int n=Options.DefaultPort; n<= Options.DefaultPort + Options.MaxPossiblePorts; ++n)
            {
                result.Add(new WebClockPortItem {Port = n});
            }

            return result;
        }

        private IEnumerable<ClockHourFormatItem> GetClockHourFormats()
        {
            return new List<ClockHourFormatItem>
            {
                new ClockHourFormatItem {Name = Properties.Resources.CLOCK_FORMAT_12, Format = ClockHourFormat.Format12},
                new ClockHourFormatItem {Name = Properties.Resources.CLOCK_FORMAT_12Z, Format = ClockHourFormat.Format12LeadingZero},
                new ClockHourFormatItem {Name = Properties.Resources.CLOCK_FORMAT_24, Format = ClockHourFormat.Format24},
                new ClockHourFormatItem {Name = Properties.Resources.CLOCK_FORMAT_24Z, Format = ClockHourFormat.Format24LeadingZero}
            };
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
                new AutoMeetingTime {Name = Properties.Resources.MIDWEEK, Id = MidWeekOrWeekend.MidWeek },
                new AutoMeetingTime {Name = Properties.Resources.WEEKEND, Id = MidWeekOrWeekend.Weekend }
            };
        }

        private IEnumerable<OperatingModeItem> GetOperatingModes()
        {
            return new List<OperatingModeItem>
            {
                new OperatingModeItem {Name = Properties.Resources.OP_MODE_MANUAL, Mode = OperatingMode.Manual},
                new OperatingModeItem {Name = Properties.Resources.OP_MODE_FILE, Mode = OperatingMode.ScheduleFile},
                new OperatingModeItem {Name = Properties.Resources.OP_MODE_AUTO, Mode = OperatingMode.Automatic}
            };
        }

        private IEnumerable<MonitorItem> GetSystemMonitors()
        {
            var result = new List<MonitorItem> { new MonitorItem() };  // empty (i.e. no timer monitor)
            result.AddRange(_monitorsService.GetSystemMonitors());
            return result;
        }

        private void NavigateOperatorPage()
        {
            Save();
            Messenger.Default.Send(new NavigateMessage(PageName, OperatorPageViewModel.PageName, null));
        }

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
                }
            }
        }

        public void Activated(object state)
        {

        }

        private void OnShutDown(ShutDownMessage obj)
        {
            Save();
        }

        private void Save()
        {
            _optionsService.Save();
        }

        public string AppVersionStr => string.Format(Properties.Resources.APP_VER, GetVersionString());

        private string GetVersionString()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
        }


        public RelayCommand NavigateOperatorCommand { get; set; }
        public RelayCommand TestBellCommand { get; set; }
        public RelayCommand OpenPortCommand { get; set; }
    }
}
