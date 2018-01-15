using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.AutoUpdates;
using OnlyT.Models;
using OnlyT.Services.Options;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;
using Serilog;

namespace OnlyT.ViewModel
{
    /// <summary>
    /// View model for the Operator page
    /// </summary>
    public class OperatorPageViewModel : ViewModelBase, IPage
    {
        public static string PageName => "OperatorPage";
        private static readonly string _arrow = "→";
        private static readonly Brush DurationBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f3dcbc"));
        private static readonly Brush DurationDimBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bba991"));
        private readonly ITalkTimerService _timerService;
        private readonly ITalkScheduleService _scheduleService;
        private readonly IOptionsService _optionsService;
        private readonly IAdaptiveTimerService _adaptiveTimerService;
        private int _secondsElapsed;
        private bool _countUp;
        private static readonly Brush WhiteBrush = Brushes.White;
        private static readonly int MaxTimerMins = 99;
        private static readonly int MaxTimerSecs = MaxTimerMins * 60;

        private readonly SolidColorBrush _bellColorActive = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f3dcbc"));
        private readonly SolidColorBrush _bellColorInactive = new SolidColorBrush(Colors.DarkGray);

        public OperatorPageViewModel(
           ITalkTimerService timerService,
           ITalkScheduleService scheduleService,
           IAdaptiveTimerService adaptiveTimerService,
           IOptionsService optionsService)
        {
            _scheduleService = scheduleService;
            _optionsService = optionsService;
            _adaptiveTimerService = adaptiveTimerService;
            _timerService = timerService;
            _timerService.TimerChangedEvent += TimerChangedHandler;
            _countUp = _optionsService.Options.CountUp;

            SelectFirstTalk();

            // commands...
            StartCommand = new RelayCommand(StartTimer, () => IsNotRunning && IsValidTalk);
            StopCommand = new RelayCommand(StopTimer, () => IsRunning);
            SettingsCommand = new RelayCommand(NavigateSettings, () => IsNotRunning);
            HelpCommand = new RelayCommand(LaunchHelp);
            NewVersionCommand = new RelayCommand(DisplayNewVersionPage);
            IncrementTimerCommand = new RelayCommand(IncrementTimer, CanIncreaseTimerValue);
            IncrementTimer15Command = new RelayCommand(IncrementTimer15Secs, CanIncreaseTimerValue);
            IncrementTimer5Command = new RelayCommand(IncrementTimer5Mins, CanIncreaseTimerValue);
            DecrementTimerCommand = new RelayCommand(DecrementTimer, CanDecreaseTimerValue);
            DecrementTimer15Command = new RelayCommand(DecrementTimer15Secs, CanDecreaseTimerValue);
            DecrementTimer5Command = new RelayCommand(DecrementTimer5Mins, CanDecreaseTimerValue);
            BellToggleCommand = new RelayCommand(BellToggle);
            CountUpToggleCommand = new RelayCommand(CountUpToggle);
            CloseCountdownCommand = new RelayCommand(CloseCountdownWindow);

            // subscriptions...
            Messenger.Default.Register<OperatingModeChangedMessage>(this, OnOperatingModeChanged);
            Messenger.Default.Register<AutoMeetingChangedMessage>(this, OnAutoMeetingChanged);
            Messenger.Default.Register<CountdownWindowStatusChangedMessage>(this, OnCountdownWindowStatusChanged);

            GetVersionData();
        }

        public bool IsCountdownActive { get; private set; }
        private void OnCountdownWindowStatusChanged(CountdownWindowStatusChangedMessage message)
        {
            IsCountdownActive = message.Showing;
            RaisePropertyChanged(nameof(IsCountdownActive));
        }

        public bool IsNewVersionAvailable { get; private set; }
        private void GetVersionData()
        {
            Task.Delay(2000).ContinueWith(_ =>
            {
                var latestVersion = VersionDetection.GetLatestReleaseVersion();
                if (latestVersion != null)
                {
                    if (latestVersion != VersionDetection.GetCurrentVersion())
                    {
                        // there is a new version....
                        IsNewVersionAvailable = true;
                        RaisePropertyChanged(nameof(IsNewVersionAvailable));
                    }
                }
            });
        }
        
        private void DisplayNewVersionPage()
        {
            Process.Start(VersionDetection.LatestReleaseUrl);
        }

        private void CountUpToggle()
        {
            if (_optionsService.Options.AllowCountUpToggle)
            {
                var talk = GetCurrentTalk();
                if (talk != null)
                {
                    talk.CountUp = !_countUp;
                    RefreshCountUpFlag(talk);
                }
            }
        }
        
        private void LaunchHelp()
        {
            Process.Start(@"https://github.com/AntonyCorbett/OnlyT/wiki");
        }

        private void BellToggle()
        {
            var talk = GetCurrentTalk();
            if (talk != null)
            {
                talk.Bell = !talk.Bell;
                RaisePropertyChanged(nameof(BellColour));
                RaisePropertyChanged(nameof(BellTooltip));
            }
        }

        public SolidColorBrush BellColour
        {
            get
            {
                var talk = GetCurrentTalk();
                if (talk != null)
                {
                    return talk.Bell
                       ? _bellColorActive
                       : _bellColorInactive;
                }
                return _bellColorInactive;
            }
        }

        public string BellTooltip
        {
            get
            {
                var talk = GetCurrentTalk();
                if (talk != null)
                {
                    return talk.Bell
                       ? Properties.Resources.ACTIVE_BELL
                       : Properties.Resources.INACTIVE_BELL;
                }

                return string.Empty;
            }
        }

        private bool CanIncreaseTimerValue()
        {
            if (IsRunning || TargetSeconds >= MaxTimerSecs)
            {
                return false;
            }

            return CurrentTalkTimerIsEditable();
        }

        private bool CanDecreaseTimerValue()
        {
            if (IsRunning || TargetSeconds <= 0)
            {
                return false;
            }

            return CurrentTalkTimerIsEditable();
        }

        private bool CurrentTalkTimerIsEditable()
        {
            var talk = GetCurrentTalk();
            return talk != null && talk.Editable;
        }

        private void OnAutoMeetingChanged(AutoMeetingChangedMessage message)
        {
            try
            {
                _scheduleService.Reset();
                TalkId = 0;
                RaisePropertyChanged(nameof(Talks));
                SelectFirstTalk();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not handle change of meeting schedule");
            }
        }

        private void IncrDecrTimerInternal(int mins)
        {
            var newSecs = TargetSeconds + mins;
            if (newSecs >= 0 && newSecs <= MaxTimerSecs)
            {
                TargetSeconds = newSecs;
                AdjustTalkTimeForThisSession();
            }
        }

        private void DecrementTimer()
        {
            IncrDecrTimerInternal(-60);
        }

        private void DecrementTimer15Secs()
        {
            IncrDecrTimerInternal(-15);
        }

        private void DecrementTimer5Mins()
        {
            IncrDecrTimerInternal(-5 * 60);
        }

        private void IncrementTimer()
        {
            IncrDecrTimerInternal(60);
        }

        private void IncrementTimer15Secs()
        {
            IncrDecrTimerInternal(15);
        }
        private void IncrementTimer5Mins()
        {
            IncrDecrTimerInternal(5 * 60);
        }

        private void OnOperatingModeChanged(OperatingModeChangedMessage message)
        {
            RaisePropertyChanged(nameof(IsManualMode));
            RaisePropertyChanged(nameof(IsNotManualMode));
            Messenger.Default.Send(new AutoMeetingChangedMessage());
        }

        private void SelectFirstTalk()
        {
            var talks = _scheduleService.GetTalkScheduleItems()?.ToArray();
            if (talks != null && talks.Any())
            {
                TalkId = talks.First().Id;
            }
        }

        private void NavigateSettings()
        {
            Messenger.Default.Send(new NavigateMessage(PageName, SettingsPageViewModel.PageName, null));
        }

        private void StopTimer()
        {
            _timerService.Stop();
            _isStarting = false;
            
            TextColor = WhiteBrush;

            RaisePropertyChanged(nameof(IsRunning));
            RaisePropertyChanged(nameof(IsNotRunning));

            Messenger.Default.Send(new TimerStopMessage());
            RaiseCanExecuteChanged();

            AutoAdvance();
        }

        private void AutoAdvance()
        {
            // advance to next item...
            TalkId = _scheduleService.GetNext(TalkId);
        }

        public bool IsManualMode => _optionsService.Options.OperatingMode == OperatingMode.Manual;
        public bool IsNotManualMode => _optionsService.Options.OperatingMode != OperatingMode.Manual;


        public bool IsRunning => _timerService.IsRunning || _isStarting;
        public bool IsNotRunning => !IsRunning;

        private bool _isStarting;

        private void StartTimer()
        {
            _isStarting = true;
            _secondsElapsed = 0;

            RunFlashAnimation = false;
            RunFlashAnimation = true;

            RaisePropertyChanged(nameof(IsRunning));
            RaisePropertyChanged(nameof(IsNotRunning));

            RaiseCanExecuteChanged();
            AdjustForAdaptiveTime();

            Messenger.Default.Send(new TimerStartMessage(_targetSeconds, _countUp));

            Task.Run(() =>
            {
                int ms = DateTime.Now.Millisecond;
                if (ms > 100)
                {
                    // sync to the second (so that the timer window clock and countdown
                    // seconds are in sync)...

                    Task.Delay(1000 - ms).Wait();
                }

                if (_isStarting)
                {
                    _timerService.Start(_targetSeconds);
                }
            });
        }

        private void AdjustForAdaptiveTime()
        {
            try
            {
                if (TalkId > 0)
                {
                    var newDuration = _adaptiveTimerService.CalculateAdaptedDuration(TalkId);
                    if (newDuration != null)
                    {
                        var talk = GetCurrentTalk();
                        if(talk != null)
                        { 
                            talk.AdaptedDuration = newDuration.Value;
                            SetDurationStringAttributes(talk);
                            TargetSeconds = (int)talk.ActualDuration.TotalSeconds;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not adjust for adaptive time");
            }
        }

        private void RaiseCanExecuteChanged()
        {
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            SettingsCommand.RaiseCanExecuteChanged();

            RaiseCanExecuteIncrDecrChanged();
        }

        private void RaiseCanExecuteIncrDecrChanged()
        {
            IncrementTimerCommand?.RaiseCanExecuteChanged();
            IncrementTimer5Command?.RaiseCanExecuteChanged();
            IncrementTimer15Command?.RaiseCanExecuteChanged();

            DecrementTimerCommand?.RaiseCanExecuteChanged();
            DecrementTimer5Command?.RaiseCanExecuteChanged();
            DecrementTimer15Command?.RaiseCanExecuteChanged();
        }

        public bool IsValidTalk => GetCurrentTalk() != null;

        public IEnumerable<TalkScheduleItem> Talks => _scheduleService.GetTalkScheduleItems();

        private int _talkId;
        public int TalkId
        {
            get => _talkId;
            set
            {
                if (_talkId != value)
                {
                    _talkId = value;

                    var talk = GetCurrentTalk();
                    RefreshCountUpFlag(talk);
                    
                    TargetSeconds = GetTargetSecondsFromTalkSchedule(talk);
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsBellVisible));
                    RaisePropertyChanged(nameof(BellColour));
                    RaisePropertyChanged(nameof(BellTooltip));
                    RaisePropertyChanged(nameof(IsValidTalk));
                    
                    RaiseCanExecuteIncrDecrChanged();
                    StartCommand?.RaiseCanExecuteChanged();
                    SetDurationStringAttributes(talk);
                }
            }
        }

        private void RefreshCountUpFlag(TalkScheduleItem talk)
        {
            _countUp = talk?.CountUp ?? _optionsService.Options.CountUp;
            RaisePropertyChanged(nameof(CountUpOrDownImageData));
            RaisePropertyChanged(nameof(CountUpOrDownTooltip));
            RaisePropertyChanged(nameof(CurrentTimerValueString));
        }

        private TalkScheduleItem GetCurrentTalk()
        {
            return _scheduleService.GetTalkScheduleItem(TalkId);
        }

        private int GetTargetSecondsFromTalkSchedule(TalkScheduleItem talk)
        {
            return talk?.GetDurationSeconds() ?? 0;
        }

        private bool _runFlashAnimation;
        public bool RunFlashAnimation
        {
            get => _runFlashAnimation;
            set
            {
                if (_runFlashAnimation != value)
                {
                    TextColor = new SolidColorBrush(Colors.White);
                    _runFlashAnimation = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _textColor = Brushes.White;
        public Brush TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                RaisePropertyChanged();
            }
        }

        private void TimerChangedHandler(object sender, EventArgs.TimerChangedEventArgs e)
        {
            TextColor = GreenYellowRedSelector.GetBrushForTimeRemaining(e.RemainingSecs);
            _secondsElapsed = e.ElapsedSecs;
            SecondsRemaining = e.RemainingSecs;
            
            Messenger.Default.Send(new TimerChangedMessage(e.RemainingSecs, e.ElapsedSecs, e.IsRunning, _countUp));

            if (e.RemainingSecs == 0)
            {
                // broadcast talk overtime...
                var talk = GetCurrentTalk();
                if (talk != null)
                {
                    Messenger.Default.Send(new OvertimeMessage(talk.Name, talk.Bell, e.TargetSecs));
                }
            }
        }

        private int _targetSeconds;
        
        private int TargetSeconds
        {
            get => _targetSeconds;
            set
            {
                if (_targetSeconds != value)
                {
                    _targetSeconds = value;
                    SecondsRemaining = _targetSeconds;
                    
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CurrentTimerValueString));
                    RaiseCanExecuteIncrDecrChanged();
                }
            }
        }

        private void SetDurationStringAttributes(TalkScheduleItem talk)
        {
            if (talk != null)
            {
                Duration1String = TimeFormatter.FormatTimerDisplayString((int)talk.OriginalDuration.TotalSeconds);
                Duration1Tooltip = Properties.Resources.DURATION_ORIGINAL;

                if (talk.ModifiedDuration != null)
                {
                    Duration2String = TimeFormatter.FormatTimerDisplayString((int) talk.ModifiedDuration.Value.TotalSeconds);
                    Duration2Tooltip = Properties.Resources.DURATION_MODIFIED;

                    if (talk.AdaptedDuration != null)
                    {
                        Duration3String = TimeFormatter.FormatTimerDisplayString((int) talk.AdaptedDuration.Value.TotalSeconds);
                        Duration3Tooltip = Properties.Resources.DURATION_ADAPTED;
                    }
                    else
                    {
                        Duration3String = string.Empty;
                    }
                }
                else if (talk.AdaptedDuration != null)
                {
                    Duration2String = TimeFormatter.FormatTimerDisplayString((int)talk.AdaptedDuration.Value.TotalSeconds);
                    Duration2Tooltip = Properties.Resources.DURATION_ADAPTED;
                    Duration3String = string.Empty;
                }
                else
                {
                    Duration2String = string.Empty;
                    Duration3String = string.Empty;
                }
            }
            else
            {
                Duration1String = string.Empty;
                Duration2String = string.Empty;
                Duration3String = string.Empty;
            }

            Duration1Colour = DurationDimBrush;
            Duration2Colour = DurationDimBrush;
            Duration3Colour = DurationDimBrush;
            
            if (!string.IsNullOrEmpty(Duration3String))
            {
                Duration3Colour = DurationBrush;
            }
            else if (!string.IsNullOrEmpty(Duration2String))
            {
                Duration2Colour = DurationBrush;
            }
            else
            {
                Duration1Colour = DurationBrush;
            }
            
            RaisePropertyChanged(nameof(Duration1Colour));
            RaisePropertyChanged(nameof(Duration2Colour));
            RaisePropertyChanged(nameof(Duration3Colour));

            RaisePropertyChanged(nameof(Duration1Tooltip));
            RaisePropertyChanged(nameof(Duration2Tooltip));
            RaisePropertyChanged(nameof(Duration3Tooltip));
        }

        private void CloseCountdownWindow()
        {
            Messenger.Default.Send(new StopCountDownMessage());
        }


        public string Duration1ArrowString { get; set; }
        public string Duration2ArrowString { get; set; }
        public Brush Duration1Colour { get; set; }
        public Brush Duration2Colour { get; set; }
        public Brush Duration3Colour { get; set; }
        public string Duration1Tooltip { get; set; }
        public string Duration2Tooltip { get; set; }
        public string Duration3Tooltip { get; set; }


        private string _duration1String;
        public string Duration1String
        {
            get => _duration1String;
            set
            {
                _duration1String = value;
                RaisePropertyChanged();
            }
        }

        private string _duration2String;
        public string Duration2String
        {
            get => _duration2String;
            set
            {
                _duration2String = value;
                
                RaisePropertyChanged();
                Duration1ArrowString = string.IsNullOrEmpty(_duration2String) ? string.Empty : _arrow;
                RaisePropertyChanged(nameof(Duration1ArrowString));
            }
        }

        private string _duration3String;
        public string Duration3String
        {
            get => _duration3String;
            set
            {
                _duration3String = value;
                RaisePropertyChanged();
                Duration2ArrowString = string.IsNullOrEmpty(_duration3String) ? string.Empty : _arrow;
                RaisePropertyChanged(nameof(Duration2ArrowString));
            }
        }

        private void AdjustTalkTimeForThisSession()
        {
            var talk = GetCurrentTalk();
            if (talk != null && talk.Editable)
            {
                talk.ModifiedDuration = TimeSpan.FromSeconds(TargetSeconds);
                SetDurationStringAttributes(talk);
            }
        }

        private int _secondsRemaining;

        private int SecondsRemaining
        {
            get => _secondsRemaining;
            set
            {
                if (_secondsRemaining != value)
                {
                    _secondsRemaining = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CurrentTimerValueString));
                }
            }
        }
        
        public string CurrentTimerValueString => TimeFormatter.FormatTimerDisplayString(_countUp
            ? _secondsElapsed
            : _secondsRemaining);

        public bool IsBellVisible
        {
            get
            {
                var talk = GetCurrentTalk();
                return talk != null && talk.OriginalBell && _optionsService.Options.IsBellEnabled;
            }
        }
        
        public bool AllowCountUpDownToggle => _optionsService.Options.AllowCountUpToggle;

        public string CountUpOrDownTooltip => _countUp
            ? "Currently counting up"
            : "Currently counting down";

        public string CountUpOrDownImageData => _countUp
            ? "M 16,0 L 32,7.5 22,7.5 16,30 10,7.5 0,7.5z"
            : "M 16,0 L 22,22.5 32,22.5 16,30 0,22.5 10,22.5z";

        public bool IsCircuitVisit => _optionsService.Options.IsCircuitVisit &&
                                      _optionsService.Options.OperatingMode == OperatingMode.Automatic;

        public void Activated(object state)
        {
            // "CountUp" setting may have changed
            var talk = GetCurrentTalk();
            RefreshCountUpFlag(talk);
            
            RaisePropertyChanged(nameof(AllowCountUpDownToggle));
            RaisePropertyChanged(nameof(IsBellVisible));
            RaisePropertyChanged(nameof(IsCircuitVisit));
        }

        public RelayCommand StartCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand SettingsCommand { get; set; }
        public RelayCommand HelpCommand { get; set; }
        public RelayCommand NewVersionCommand { get; set; }
        public RelayCommand IncrementTimerCommand { get; set; }
        public RelayCommand IncrementTimer15Command { get; set; }
        public RelayCommand IncrementTimer5Command { get; set; }
        public RelayCommand DecrementTimerCommand { get; set; }
        public RelayCommand DecrementTimer15Command { get; set; }
        public RelayCommand DecrementTimer5Command { get; set; }
        public RelayCommand BellToggleCommand { get; set; }
        public RelayCommand CountUpToggleCommand { get; set; }
        public RelayCommand CloseCountdownCommand { get; set; }
    }
}
