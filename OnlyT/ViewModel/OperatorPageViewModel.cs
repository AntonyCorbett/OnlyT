namespace OnlyT.ViewModel
{
    // ReSharper disable CatchAllClause
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using AutoUpdates;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.CommandWpf;
    using GalaSoft.MvvmLight.Messaging;
    using Messages;
    using Models;
    using OnlyT.Services.Automate;
    using OnlyT.Services.Report;
    using OnlyT.Services.Snackbar;
    using Serilog;
    using Services.Bell;
    using Services.CommandLine;
    using Services.Options;
    using Services.TalkSchedule;
    using Services.Timer;
    using Utils;
    using WebServer.ErrorHandling;

    /// <summary>
    /// View model for the Operator page
    /// </summary>
    public class OperatorPageViewModel : ViewModelBase, IPage
    {
        private static readonly string Arrow = "→";
        
        // ReSharper disable once PossibleNullReferenceException
        private static readonly Brush DurationBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f3dcbc"));
        
        // ReSharper disable once PossibleNullReferenceException
        private static readonly Brush DurationDimBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bba991"));

        private static readonly Brush WhiteBrush = Brushes.White;
        private static readonly int MaxTimerMins = 99;
        private static readonly int MaxTimerSecs = MaxTimerMins * 60;

        // ReSharper disable once PossibleNullReferenceException
        private readonly SolidColorBrush _bellColorActive = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f3dcbc"));

        private readonly SolidColorBrush _bellColorInactive = new SolidColorBrush(Colors.DarkGray);

        private readonly ITalkTimerService _timerService;
        private readonly ITalkScheduleService _scheduleService;
        private readonly IOptionsService _optionsService;
        private readonly IAdaptiveTimerService _adaptiveTimerService;
        private readonly IBellService _bellService;
        private readonly ICommandLineService _commandLineService;
        private readonly ILocalTimingDataStoreService _timingDataService;
        private readonly ISnackbarService _snackbarService;

        private int _secondsElapsed;
        private bool _countUp;
        private bool _isStarting;
        private int _talkId;
        private bool _runFlashAnimation;
        private Brush _textColor = Brushes.White;
        private int _targetSeconds;
        private string _duration1String;
        private string _duration2String;
        private string _duration3String;
        private int _secondsRemaining;
        private DateTime? _meetingStartTimeFromCountdown;

        public OperatorPageViewModel(
           ITalkTimerService timerService,
           ITalkScheduleService scheduleService,
           IAdaptiveTimerService adaptiveTimerService,
           IOptionsService optionsService,
           ICommandLineService commandLineService,
           IBellService bellService,
           ILocalTimingDataStoreService timingDataService,
           ISnackbarService snackbarService)
        {
            _scheduleService = scheduleService;
            _optionsService = optionsService;
            _adaptiveTimerService = adaptiveTimerService;
            _commandLineService = commandLineService;
            _bellService = bellService;
            _timerService = timerService;
            _snackbarService = snackbarService;
            _timingDataService = timingDataService;
            _timerService.TimerChangedEvent += TimerChangedHandler;
            _countUp = _optionsService.Options.CountUp;

            SelectFirstTalk();

            _timerService.TimerStartStopFromApiEvent += HandleTimerStartStopFromApi;

            // commands...
            StartCommand = new RelayCommand(StartTimer, () => IsNotRunning && IsValidTalk, true);
            StopCommand = new RelayCommand(StopTimer, () => IsRunning);
            SettingsCommand = new RelayCommand(NavigateSettings, () => IsNotRunning && !_commandLineService.NoSettings);
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
            Messenger.Default.Register<ShowCircuitVisitToggleChangedMessage>(this, OnShowCircuitVisitToggleChanged);

            if (IsInDesignMode)
            {
                IsNewVersionAvailable = true;
            }

            GetVersionData();

            if (commandLineService.Automate)
            {
#if DEBUG
                var automationService = new AutomateService(_optionsService, _timerService, scheduleService);
                automationService.Execute();
#endif
            }
        }
        
        public static string PageName => "OperatorPage";

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
            ? Properties.Resources.COUNTING_UP
            : Properties.Resources.COUNTING_DOWN;

        public string CountUpOrDownImageData => _countUp
            ? "M 16,0 L 32,7.5 22,7.5 16,30 10,7.5 0,7.5z"
            : "M 16,0 L 22,22.5 32,22.5 16,30 0,22.5 10,22.5z";

        public bool IsCircuitVisit
        {
            get => _optionsService.Options.IsCircuitVisit &&
                   _optionsService.Options.OperatingMode == OperatingMode.Automatic;
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

        public bool IsCountdownActive { get; private set; }

        public bool IsNewVersionAvailable { get; private set; }

        public string Duration1ArrowString { get; set; }

        public string Duration2ArrowString { get; set; }

        public Brush Duration1Colour { get; set; }

        public Brush Duration2Colour { get; set; }

        public Brush Duration3Colour { get; set; }

        public string Duration1Tooltip { get; set; }

        public string Duration2Tooltip { get; set; }

        public string Duration3Tooltip { get; set; }

        public string Duration1String
        {
            get => _duration1String;
            set
            {
                _duration1String = value;
                RaisePropertyChanged();
            }
        }

        public string Duration2String
        {
            get => _duration2String;
            set
            {
                _duration2String = value;

                RaisePropertyChanged();
                Duration1ArrowString = string.IsNullOrEmpty(_duration2String) ? string.Empty : Arrow;
                RaisePropertyChanged(nameof(Duration1ArrowString));
            }
        }

        public string Duration3String
        {
            get => _duration3String;
            set
            {
                _duration3String = value;
                RaisePropertyChanged();
                Duration2ArrowString = string.IsNullOrEmpty(_duration3String) ? string.Empty : Arrow;
                RaisePropertyChanged(nameof(Duration2ArrowString));
            }
        }

        public Brush TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                RaisePropertyChanged();
            }
        }

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

                    RaiseCanExecuteIncrementDecrementChanged();
                    StartCommand?.RaiseCanExecuteChanged();
                    SetDurationStringAttributes(talk);

                    _timerService.SetupTalk(_talkId, TargetSeconds);
                }
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

        public string SettingsHint =>
            _commandLineService.NoSettings
                ? Properties.Resources.NOT_AVAIL_ADMIN
                : IsRunning
                    ? Properties.Resources.NOT_AVAIL_TIMER_RUNNING
                    : Properties.Resources.SETTINGS;

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

        public bool ShouldShowCircuitVisitToggle => IsAutoMode && _optionsService.Options.ShowCircuitVisitToggle;
        
        public bool IsManualMode => _optionsService.Options.OperatingMode == OperatingMode.Manual;

        public bool IsNotManualMode => _optionsService.Options.OperatingMode != OperatingMode.Manual;
        
        public bool IsRunning => _timerService.IsRunning || _isStarting;

        public bool IsNotRunning => !IsRunning;

        public bool IsValidTalk => GetCurrentTalk() != null;

        public IEnumerable<TalkScheduleItem> Talks => _scheduleService.GetTalkScheduleItems();

        private bool IsAutoMode => _optionsService.Options.OperatingMode == OperatingMode.Automatic;

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
                    RaiseCanExecuteIncrementDecrementChanged();
                }
            }
        }

        private int SecondsRemaining
        {
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

        public void Activated(object state)
        {
            Log.Logger.Debug("Operator page activated");

            // "CountUp" setting may have changed
            var talk = GetCurrentTalk();
            RefreshCountUpFlag(talk);

            RaisePropertyChanged(nameof(AllowCountUpDownToggle));
            RaisePropertyChanged(nameof(IsBellVisible));
            RaisePropertyChanged(nameof(IsCircuitVisit));
        }

        private void StartTimer()
        {
            Log.Logger.Debug("Starting timer");

            _isStarting = true;
            _secondsElapsed = 0;

            RunFlashAnimation = false;
            RunFlashAnimation = true;

            RaisePropertyChanged(nameof(IsRunning));
            RaisePropertyChanged(nameof(IsNotRunning));
            RaisePropertyChanged(nameof(SettingsHint));

            RaiseCanExecuteChanged();
            AdjustForAdaptiveTime();

            var talkId = TalkId;

            Messenger.Default.Send(new TimerStartMessage(_targetSeconds, _countUp, talkId));
            StoreTimerStartData();

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
                    _timerService.Start(_targetSeconds, talkId);
                }
            });
        }

        private void StoreTimerStartData()
        {
            if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
            {
                Log.Logger.Debug("Storing timer start data");

                var talk = GetCurrentTalk();

                if (IsFirstTalk(talk.Id))
                {
                    StoreTimerDataForStartOfMeeting();
                }

                if (IsFirstTalkAfterInterim(talk.Id))
                {
                    var prevTalk = GetPreviousTalk(talk.Id);
                    StoreTimerDataForInterim(prevTalk?.IsStudentTalk ?? false);
                }

                _timingDataService.InsertTimerStart(
                    talk.Name, 
                    false,
                    talk.IsStudentTalk, 
                    talk.OriginalDuration, 
                    talk.ActualDuration);
            }
        }

        private void StoreTimerDataForInterim(bool allowForCounselTime)
        {
            Log.Logger.Debug("Storing timer data for interim segment");

            var lastItemStop = _timingDataService.LastTimerStop;
            var interimStart = lastItemStop.AddSeconds(allowForCounselTime ? 75 : 15);

            // we plan 3 mins 20 secs for interim song...
            _timingDataService.InsertSongSegment(
                interimStart,
                Properties.Resources.INTERIM_SEGMENT,
                new TimeSpan(0, 3, 20));
        }

        private bool IsFirstTalkAfterInterim(int talkId)
        {
            var talkType = (TalkTypesAutoMode)talkId;

            return talkType == TalkTypesAutoMode.LivingPart1 ||
                   talkType == TalkTypesAutoMode.Watchtower;
        }

        private void StoreTimerDataForStartOfMeeting()
        {
            Log.Logger.Debug("Storing timer data for introductory segment");

            // insert start of meeting...
            var startTime = CalculateStartOfMeeting();
            _timingDataService.InsertMeetingStart(startTime);

            const int totalMtgLengthMins = 105;

            // and planned end...
            var plannedEndTime = startTime.AddMinutes(totalMtgLengthMins);
            _timingDataService.InsertPlannedMeetingEnd(plannedEndTime);

            _timingDataService.InsertSongSegment(
                startTime,
                Properties.Resources.INTRO_SEGMENT,
                TimeSpan.FromMinutes(5));
        }

        private DateTime CalculateStartOfMeeting()
        {
            var estimated = DateUtils.GetNearestQuarterOfAnHour(DateTime.Now);
            if (_meetingStartTimeFromCountdown == null)
            {
                return estimated;
            }

            var diff = estimated - _meetingStartTimeFromCountdown.Value;
            var toleranceSeconds = TimeSpan.FromMinutes(10).TotalSeconds;

            if (Math.Abs(diff.TotalSeconds) < toleranceSeconds)
            {
                return _meetingStartTimeFromCountdown.Value;
            }

            return estimated;
        }

        private void StoreTimerStopData()
        {
            if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
            {
                Log.Logger.Debug("Storing timer stop data");

                _timingDataService.InsertTimerStop();
            }
        }

        private void StoreEndOfMeetingData()
        {
            if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
            {
                Log.Logger.Debug("Storing end of meeting timer data");

                var songStart = DateTime.Now.AddSeconds(5);

                // a guess!
                var actualMeetingEnd = songStart.AddMinutes(5);

                _timingDataService.InsertConcludingSongSegment(songStart, actualMeetingEnd, Properties.Resources.CONCLUDING_SEGMENT, TimeSpan.FromMinutes(5));
                _timingDataService.InsertActualMeetingEnd(actualMeetingEnd);
            }
        }

        private void AdjustForAdaptiveTime()
        {
            try
            {
                if (TalkId > 0 && IsNotManualMode)
                {
                    var newDuration = _adaptiveTimerService.CalculateAdaptedDuration(TalkId);
                    if (newDuration != null)
                    {
                        var talk = GetCurrentTalk();
                        if (talk != null)
                        {
                            Log.Logger.Debug($"Adjusting item for adaptive time. New duration = {newDuration.Value}");

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

            RaiseCanExecuteIncrementDecrementChanged();
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
        
        private void TimerChangedHandler(object sender, OnlyT.EventArgs.TimerChangedEventArgs e)
        {
            TextColor = GreenYellowRedSelector.GetBrushForTimeRemaining(e.RemainingSecs);
            _secondsElapsed = e.ElapsedSecs;
            SecondsRemaining = e.RemainingSecs;
            
            Messenger.Default.Send(new TimerChangedMessage(e.RemainingSecs, e.ElapsedSecs, e.IsRunning, _countUp));

            if (e.RemainingSecs == 0)
            {
                var talk = GetCurrentTalk();
                if (talk != null)
                {
                    if (talk.Bell && _optionsService.Options.IsBellEnabled)
                    {
                        _bellService.Play(_optionsService.Options.BellVolumePercent);
                    }
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
                    Duration2String = TimeFormatter.FormatTimerDisplayString((int)talk.ModifiedDuration.Value.TotalSeconds);
                    Duration2Tooltip = Properties.Resources.DURATION_MODIFIED;

                    if (talk.AdaptedDuration != null)
                    {
                        Duration3String = TimeFormatter.FormatTimerDisplayString((int)talk.AdaptedDuration.Value.TotalSeconds);
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

        private void AdjustTalkTimeForThisSession()
        {
            var talk = GetCurrentTalk();
            if (talk != null && talk.Editable)
            {
                var modifiedDuration = TimeSpan.FromSeconds(TargetSeconds);

                Log.Logger.Debug($"Talk timer adjusted for this session. Modified duration = {modifiedDuration}");

                if (IsManualMode)
                {
                    talk.OriginalDuration = modifiedDuration;
                }
                else
                {
                    talk.ModifiedDuration = modifiedDuration;
                }
                
                SetDurationStringAttributes(talk);
            }
        }

        private void CloseCountdownWindow()
        {
            Log.Logger.Debug("Sending StopCountDownMessage");
            Messenger.Default.Send(new StopCountDownMessage());
        }

        private void OnShowCircuitVisitToggleChanged(ShowCircuitVisitToggleChangedMessage message)
        {
            RaisePropertyChanged(nameof(ShouldShowCircuitVisitToggle));
        }

        private void OnCountdownWindowStatusChanged(CountdownWindowStatusChangedMessage message)
        {
            if (message.Showing)
            {
                _meetingStartTimeFromCountdown = null;
            }
            else
            {
                _meetingStartTimeFromCountdown = DateUtils.GetNearestMinute(DateTime.Now);
            }

            IsCountdownActive = message.Showing;
            RaisePropertyChanged(nameof(IsCountdownActive));
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
                Log.Logger.Debug("Meeting schedule changing");

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

        private void IncrementDecrementTimerInternal(int mins)
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
            IncrementDecrementTimerInternal(-60);
        }

        private void DecrementTimer15Secs()
        {
            IncrementDecrementTimerInternal(-15);
        }

        private void DecrementTimer5Mins()
        {
            IncrementDecrementTimerInternal(-5 * 60);
        }

        private void IncrementTimer()
        {
            IncrementDecrementTimerInternal(60);
        }

        private void IncrementTimer15Secs()
        {
            IncrementDecrementTimerInternal(15);
        }

        private void IncrementTimer5Mins()
        {
            IncrementDecrementTimerInternal(5 * 60);
        }

        private void OnOperatingModeChanged(OperatingModeChangedMessage message)
        {
            Log.Logger.Debug("Responding to change in operating mode");

            RaisePropertyChanged(nameof(IsAutoMode));
            RaisePropertyChanged(nameof(ShouldShowCircuitVisitToggle));
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

        private bool IsFirstTalk(int talkId)
        {
            var talks = _scheduleService.GetTalkScheduleItems()?.ToArray();
            return talks != null && talks.Any() && talkId == talks.First().Id;
        }

        private TalkScheduleItem GetPreviousTalk(int talkId)
        {
            var talks = _scheduleService.GetTalkScheduleItems()?.ToArray();
            if (talks != null)
            {
                TalkScheduleItem prevTalk = null;
                foreach (var talk in talks)
                {
                    if (talk.Id == talkId)
                    {
                        break;
                    }

                    prevTalk = talk;
                }

                return prevTalk;
            }

            return null;
        }

        private void NavigateSettings()
        {
            Messenger.Default.Send(new NavigateMessage(PageName, SettingsPageViewModel.PageName, null));
        }

        private async void StopTimer()
        {
            Log.Logger.Debug("Stopping timer");

            var talk = GetCurrentTalk();
            var msg = new TimerStopMessage(
                TalkId, 
                _timerService.CurrentSecondsElapsed, 
                _optionsService.Options.PersistStudentTime && talk.PersistFinalTimerValue);
            
            _timerService.Stop();
            _isStarting = false;

            StoreTimerStopData();

            TextColor = WhiteBrush;

            RaisePropertyChanged(nameof(IsRunning));
            RaisePropertyChanged(nameof(IsNotRunning));
            RaisePropertyChanged(nameof(SettingsHint));

            Messenger.Default.Send(msg);
            RaiseCanExecuteChanged();

            AutoAdvance();

            if (TalkId == 0)
            {
                // end of the meeting.
                StoreEndOfMeetingData();
                await GenerateTimingReportAsync().ConfigureAwait(false);
            }
        }

        private void AutoAdvance()
        {
            // advance to next item...
            TalkId = _scheduleService.GetNext(TalkId);
        }

        private void GetVersionData()
        {
            if (!IsNewVersionAvailable)
            {
                Task.Delay(2000).ContinueWith(_ =>
                {
                    var latestVersion = VersionDetection.GetLatestReleaseVersion();
                    if (latestVersion != null)
                    {
                        if (latestVersion > VersionDetection.GetCurrentVersion())
                        {
                            // there is a new version....
                            IsNewVersionAvailable = true;
                            RaisePropertyChanged(nameof(IsNewVersionAvailable));
                        }
                    }
                });
            }
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

        private void HandleTimerStartStopFromApi(object sender, OnlyT.EventArgs.TimerStartStopEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // always on UI thread to prevent synchronisation issues.
                Log.Logger.Debug("Handling timer control from API");

                CheckTalkExists(e.TalkId);

                bool success = TalkId == e.TalkId || IsNotRunning;
                
                if (success)
                {
                    TalkId = e.TalkId;
                    success = TalkId == e.TalkId;

                    if (success)
                    {
                        switch (e.Command)
                        {
                            case StartStopTimerCommands.Start:
                                success = IsNotRunning;
                                if (success)
                                {
                                    StartTimer();
                                }

                                break;

                            case StartStopTimerCommands.Stop:
                                success = IsRunning;
                                if (success)
                                {
                                    StopTimer();
                                }

                                break;
                        }
                    }
                }
                
                e.CurrentStatus = _timerService.GetStatus();
                if (success)
                {
                    e.CurrentStatus.IsRunning = e.Command == StartStopTimerCommands.Start;
                }

                e.Success = success;
            });
        }

        private void CheckTalkExists(int talkId)
        {
            var talk = _scheduleService.GetTalkScheduleItem(talkId);
            if (talk == null)
            {
                throw new WebServerException(WebServerErrorCode.TimerDoesNotExist);
            }
        }

        private void RaiseCanExecuteIncrementDecrementChanged()
        {
            IncrementTimerCommand?.RaiseCanExecuteChanged();
            IncrementTimer5Command?.RaiseCanExecuteChanged();
            IncrementTimer15Command?.RaiseCanExecuteChanged();

            DecrementTimerCommand?.RaiseCanExecuteChanged();
            DecrementTimer5Command?.RaiseCanExecuteChanged();
            DecrementTimer15Command?.RaiseCanExecuteChanged();
        }

        private async Task GenerateTimingReportAsync()
        {
            if (_optionsService.Options.OperatingMode == OperatingMode.Automatic &&
                _optionsService.Options.GenerateTimingReports)
            {
                Log.Logger.Debug("Generating timer report");

                _snackbarService.EnqueueWithOk(Properties.Resources.ANALYSING_REPORT_DATA);

                _timingDataService.Save();

                var reportPath = await TimingReportGeneration.ExecuteAsync(
                    _timingDataService, 
                    _commandLineService.OptionsIdentifier).ConfigureAwait(false);

                if (string.IsNullOrEmpty(reportPath))
                {
                    _snackbarService.EnqueueWithOk(Properties.Resources.NO_REPORT);
                }
                else
                {
                    _snackbarService.Enqueue(
                        Properties.Resources.GENERATING_REPORT,
                        Properties.Resources.VIEW_REPORT,
                        () => LaunchPdf(reportPath));
                }
            }
        }

        private void LaunchPdf(string pdf)
        {
            if (File.Exists(pdf))
            {
                Process.Start(pdf);
            }
        }
    }
}
