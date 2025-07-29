// ReSharper disable CatchAllClause

// Ignore Spelling: snackbar

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using OnlyT.AutoUpdates;
using OnlyT.Common.Services.DateTime;
using OnlyT.EventTracking;
using OnlyT.Models;
using OnlyT.Services.Automate;
using OnlyT.Services.Bell;
using OnlyT.Services.CommandLine;
using OnlyT.Services.Options;
using OnlyT.Services.OverrunNotificationService;
using OnlyT.Services.Report;
using OnlyT.Services.Snackbar;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;
using OnlyT.WebServer.ErrorHandling;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace OnlyT.ViewModel;

/// <summary>
/// View model for the Operator page
/// </summary>
public class OperatorPageViewModel : ObservableObject, IPage
{
    private static readonly string Arrow = "→";
        
    private static readonly SolidColorBrush WhiteBrush = Brushes.White;
    private static readonly int MaxTimerMins = 99;
    private static readonly int MaxTimerSecs = MaxTimerMins * 60;

    // ReSharper disable once PossibleNullReferenceException
    private readonly SolidColorBrush _bellColorActive = new((Color)ColorConverter.ConvertFromString("#f3dcbc"));

    private readonly SolidColorBrush _bellColorInactive = new(Colors.DarkGray);

    private readonly SolidColorBrush _bellColorManual = new(Colors.Red);

    private readonly ITalkTimerService _timerService;
    private readonly ITalkScheduleService _scheduleService;
    private readonly IOptionsService _optionsService;
    private readonly IAdaptiveTimerService _adaptiveTimerService;
    private readonly IBellService _bellService;
    private readonly ICommandLineService _commandLineService;
    private readonly ILocalTimingDataStoreService _timingDataService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ISnackbarService _snackbarService;
    private readonly IQueryWeekendService _queryWeekendService;
    private readonly IOverrunService _overrunService;

    private int _secondsElapsed;
    private bool _countUp;
    private bool _isStarting;
    private int _talkId;
    private bool _runFlashAnimation;
    private Brush _textColor = Brushes.White;
    private int _targetSeconds;
    private string? _duration1String;
    private string? _duration2String;
    private string? _duration3String;
    private int _secondsRemaining;
    private DateTime? _meetingStartTimeFromCountdown;
    private bool _isOvertime;
    private bool _isShrunk;

    public OperatorPageViewModel(
        ITalkTimerService timerService,
        ITalkScheduleService scheduleService,
        IAdaptiveTimerService adaptiveTimerService,
        IOptionsService optionsService,
        ICommandLineService commandLineService,
        IBellService bellService,
        ILocalTimingDataStoreService timingDataService,
        ISnackbarService snackbarService,
        IDateTimeService dateTimeService,
        IQueryWeekendService queryWeekendService,
        IOverrunService overrunService)
    {
        _scheduleService = scheduleService;
        _optionsService = optionsService;
        _adaptiveTimerService = adaptiveTimerService;
        _commandLineService = commandLineService;
        _bellService = bellService;
        _timerService = timerService;
        _snackbarService = snackbarService;
        _timingDataService = timingDataService;
        _dateTimeService = dateTimeService;
        _queryWeekendService = queryWeekendService;
        _overrunService = overrunService;

        _timerService.TimerChangedEvent += TimerChangedHandler;
        _countUp = _optionsService.Options.CountUp;

        SelectFirstTalk();

        _timerService.TimerStartStopFromApiEvent += HandleTimerStartStopFromApi;

        // commands...
        StartCommand = new RelayCommand(StartTimer, () => IsNotRunning && IsValidTalk);
        StopCommand = new RelayCommand(StopTimer, () => IsRunning);
        SettingsCommand = new RelayCommand(NavigateSettings, () => IsNotRunning && !_commandLineService.NoSettings);
        CloseAppCommand = new RelayCommand(CloseApp, () => IsNotRunning);
        ExpandFromShrinkCommand = new RelayCommand(ExpandFromShrink);
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
        WeakReferenceMessenger.Default.Register<OperatingModeChangedMessage>(this, OnOperatingModeChanged);
        WeakReferenceMessenger.Default.Register<AutoMeetingChangedMessage>(this, OnAutoMeetingChanged);
        WeakReferenceMessenger.Default.Register<CountdownWindowStatusChangedMessage>(this, OnCountdownWindowStatusChanged);
        WeakReferenceMessenger.Default.Register<ShowCircuitVisitToggleChangedMessage>(this, OnShowCircuitVisitToggleChanged);
        WeakReferenceMessenger.Default.Register<AutoBellSettingChangedMessage>(this, OnAutoBellSettingChanged);
        WeakReferenceMessenger.Default.Register<RefreshScheduleMessage>(this, OnRefreshSchedule);
        WeakReferenceMessenger.Default.Register<MainWindowSizeChangedMessage>(this, OnWindowSizeChanged);

        GetVersionData();

        if (commandLineService.Automate)
        {
#if DEBUG
            var automationService = new AutomateService(
                _optionsService, 
                _timerService, 
                scheduleService,
                dateTimeService);

            automationService.Execute();

            _snackbarService.Enqueue("Automate starts on the nearest 1/4 hr");
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
            return talk != null && talk.BellApplicable && _optionsService.Options.IsBellEnabled;
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
                OnPropertyChanged();
                WeakReferenceMessenger.Default.Send(new AutoMeetingChangedMessage());
            }
        }
    }

    public int StartStopButtonRowSpan => InShrinkMode ? 2 : 1;

    public int StartStopButtonHeight => InShrinkMode ? 110 : 54;

    public int TimeDisplayColumnSpan => InShrinkMode ? 2 : 1;

    public RelayCommand StartCommand { get; }

    public RelayCommand StopCommand { get; }

    public RelayCommand SettingsCommand { get; }

    public RelayCommand CloseAppCommand { get; }

    public RelayCommand ExpandFromShrinkCommand { get; }

    public RelayCommand HelpCommand { get; }

    public RelayCommand NewVersionCommand { get; }

    public RelayCommand IncrementTimerCommand { get; }

    public RelayCommand IncrementTimer15Command { get; }

    public RelayCommand IncrementTimer5Command { get; }

    public RelayCommand DecrementTimerCommand { get; }

    public RelayCommand DecrementTimer15Command { get; }

    public RelayCommand DecrementTimer5Command { get; }

    public RelayCommand BellToggleCommand { get; }

    public RelayCommand CountUpToggleCommand { get; }

    public RelayCommand CloseCountdownCommand { get; }

    public bool RunFlashAnimation
    {
        get => _runFlashAnimation;
        set
        {
            if (_runFlashAnimation != value)
            {
                TextColor = new SolidColorBrush(Colors.White);
                _runFlashAnimation = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsCountdownActive { get; private set; }

    public bool IsNewVersionAvailable { get; private set; }

    public string? Duration1ArrowString { get; set; }

    public string? Duration2ArrowString { get; set; }

    public Brush? Duration1Colour { get; set; }

    public Brush? Duration2Colour { get; set; }

    public Brush? Duration3Colour { get; set; }

    public string? Duration1Tooltip { get; set; }

    public string? Duration2Tooltip { get; set; }

    public string? Duration3Tooltip { get; set; }

    public string? Duration1String
    {
        get => _duration1String;
        set
        {
            _duration1String = value;
            OnPropertyChanged();
        }
    }

    public string? Duration2String
    {
        get => _duration2String;
        set
        {
            _duration2String = value;

            OnPropertyChanged();
            Duration1ArrowString = string.IsNullOrEmpty(_duration2String) ? string.Empty : Arrow;
            OnPropertyChanged(nameof(Duration1ArrowString));
        }
    }

    public string? Duration3String
    {
        get => _duration3String;
        set
        {
            _duration3String = value;

            OnPropertyChanged();
            Duration2ArrowString = string.IsNullOrEmpty(_duration3String) ? string.Empty : Arrow;
            OnPropertyChanged(nameof(Duration2ArrowString));
        }
    }

    public bool IsOvertime
    {
        get => _isOvertime;
        set
        {
            if (_isOvertime != value)
            {
                _isOvertime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BellColour));
                OnPropertyChanged(nameof(BellTooltip));
            }
        }
    }

    public Brush TextColor
    {
        get => _textColor;
        set
        {
            _textColor = value;
            OnPropertyChanged();
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

                TargetSeconds = GetPlannedSecondsFromTalkSchedule(talk);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsBellVisible));
                OnPropertyChanged(nameof(BellColour));
                OnPropertyChanged(nameof(BellTooltip));
                OnPropertyChanged(nameof(IsValidTalk));
                OnPropertyChanged(nameof(ShowUpDownButton));

                RaiseCanExecuteIncrementDecrementChanged();
                StartCommand?.NotifyCanExecuteChanged();
                SetDurationStringAttributes(talk);

                IsOvertime = false;

                _timerService.SetupTalk(_talkId, TargetSeconds, talk?.ClosingSecs ?? TalkScheduleItem.DefaultClosingSecs);
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
                if (IsOvertime)
                {
                    return _bellColorManual;
                }

                return talk.AutoBell
                    ? _bellColorActive
                    : _bellColorInactive;
            }

            return _bellColorInactive;
        }
    }

    public string SettingsHint
    {
        get
        {
            if (_commandLineService.NoSettings)
            {
                return Properties.Resources.NOT_AVAIL_ADMIN;
            }

            return IsRunning
                ? Properties.Resources.NOT_AVAIL_TIMER_RUNNING
                : Properties.Resources.SETTINGS;
        }
    }

    public string BellTooltip
    {
        get
        {
            var talk = GetCurrentTalk();
            if (talk != null)
            {
                if (_isOvertime)
                {
                    return Properties.Resources.SOUND_BELL;
                }

                return talk.AutoBell
                    ? Properties.Resources.ACTIVE_BELL
                    : Properties.Resources.INACTIVE_BELL;
            }

            return string.Empty;
        }
    }

    public bool ShouldShowCircuitVisitToggle => IsAutoMode && _optionsService.Options.ShowCircuitVisitToggle;

    public bool InShrinkMode => _isShrunk;

    public bool NotInShrinkMode => !InShrinkMode;
            
    public bool IsManualMode => _optionsService.Options.OperatingMode == OperatingMode.Manual;

    public bool IsNotManualMode => _optionsService.Options.OperatingMode != OperatingMode.Manual;
        
    public bool IsRunning => _timerService.IsRunning || _isStarting;

    public bool IsNotRunning => !IsRunning;

    public bool IsValidTalk => GetCurrentTalk() != null;

    public bool ShowUpDownButton => IsValidTalk && NotInShrinkMode;

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
                SetSecondsRemaining(_targetSeconds);

                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentTimerValueString));
                RaiseCanExecuteIncrementDecrementChanged();
            }
        }
    }

    public void Activated(object? state)
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Operator page activated");
        }

        // "CountUp" setting may have changed
        var talk = GetCurrentTalk();
        RefreshCountUpFlag(talk);

        OnPropertyChanged(nameof(AllowCountUpDownToggle));
        OnPropertyChanged(nameof(IsBellVisible));
        OnPropertyChanged(nameof(IsCircuitVisit));
    }

    private void StartTimer()
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Starting timer");
        }
        
        EventTracker.Track(EventName.StartingTimer);

        _isStarting = true;
        _secondsElapsed = 0;

        RunFlashAnimation = false;
        RunFlashAnimation = true;

        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsNotRunning));
        OnPropertyChanged(nameof(SettingsHint));

        RaiseCanExecuteChanged();
        AdjustForAdaptiveTime();

        var talkId = TalkId;

        WeakReferenceMessenger.Default.Send(new TimerStartMessage(_targetSeconds, _countUp, talkId));
        StoreTimerStartData();

        Task.Run(() =>
        {
            var ms = _dateTimeService.Now().Millisecond;
            if (ms > 100)
            {
                // sync to the second (so that the timer window clock and countdown
                // seconds are in sync)...
                Task.Delay(1000 - ms).Wait();
            }

            if (_isStarting)
            {
                _timerService.Start(_targetSeconds, talkId, _countUp);
            }
        });
    }

    private void StoreTimerStartData()
    {
        if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
        {
            var talk = GetCurrentTalk();
            if (talk == null)
            {
                return;
            }

            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Storing timer start data");
            }

            if (IsFirstTalk(talk.Id))
            {
                StoreTimerDataForStartOfMeeting();
            }

            if (IsFirstTalkAfterInterval(talk.Id))
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
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Storing timer data for interim segment");
        }

        var lastItemStop = _timingDataService.LastTimerStop;
        var interimStart = lastItemStop.AddSeconds(allowForCounselTime ? 75 : 15);

        // we plan 3 mins 20 secs for interim song...
        _timingDataService.InsertSongSegment(
            interimStart,
            Properties.Resources.INTERIM_SEGMENT,
            new TimeSpan(0, 3, 20));
    }

    private static bool IsFirstTalkAfterInterval(int talkId)
    {
        var talkType = (TalkTypesAutoMode)talkId;

        return talkType == TalkTypesAutoMode.LivingPart1 ||
               talkType == TalkTypesAutoMode.Watchtower;
    }

    private void StoreTimerDataForStartOfMeeting()
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Storing timer data for introductory segment");
        }

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
        var estimated = DateUtils.GetNearestQuarterOfAnHour(_dateTimeService.Now());
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
            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Storing timer stop data");
            }

            _timingDataService.InsertTimerStop();
        }
    }

    private void StoreEndOfMeetingData()
    {
        if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
        {
            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Storing end of meeting timer data");
            }

            var songStart = _dateTimeService.Now().AddSeconds(5);

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
            if (TalkId > 0 && IsAutoMode)
            {
                var newDuration = _adaptiveTimerService.CalculateAdaptedDuration(TalkId);
                if (newDuration != null)
                {
                    if (Log.IsEnabled(LogEventLevel.Debug))
                    {
                        Log.Logger.Debug("New duration = {NewDuration}", newDuration.Value);
                    }

                    var talk = GetCurrentTalk();
                    if (talk != null)
                    {
                        if (Log.IsEnabled(LogEventLevel.Debug))
                        {
                            Log.Logger.Debug("Adjusting item for adaptive time. New duration = {NewDuration}", newDuration.Value);
                        }

                        talk.AdaptedDuration = newDuration.Value;
                        SetDurationStringAttributes(talk);
                        TargetSeconds = (int)talk.ActualDuration.TotalSeconds;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not adjust for adaptive time";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    private void RaiseCanExecuteChanged()
    {
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
        SettingsCommand.NotifyCanExecuteChanged();

        RaiseCanExecuteIncrementDecrementChanged();
    }
        
    private void RefreshCountUpFlag(TalkScheduleItem? talk)
    {
        _countUp = talk?.CountUp ?? _optionsService.Options.CountUp;
        OnPropertyChanged(nameof(CountUpOrDownImageData));
        OnPropertyChanged(nameof(CountUpOrDownTooltip));
        OnPropertyChanged(nameof(CurrentTimerValueString));
    }

    private TalkScheduleItem? GetCurrentTalk()
    {
        return _scheduleService.GetTalkScheduleItem(TalkId);
    }

    private static int GetPlannedSecondsFromTalkSchedule(TalkScheduleItem? talk)
    {
        return talk?.GetPlannedDurationSeconds() ?? 0;
    }
        
    private void TimerChangedHandler(object? sender, OnlyT.EventArgs.TimerChangedEventArgs e)
    {
        TextColor = GreenYellowRedSelector.GetBrushForTimeRemaining(e.RemainingSecs, e.ClosingSecs);
        _secondsElapsed = e.ElapsedSecs;
        SetSecondsRemaining(e.RemainingSecs);

        WeakReferenceMessenger.Default.Send(new TimerChangedMessage(e.RemainingSecs, e.ElapsedSecs, e.IsRunning, e.ClosingSecs, _countUp));

        if (e.RemainingSecs == 0)
        {
            IsOvertime = true;

            var talk = GetCurrentTalk();
            if (talk != null && _optionsService.Options.IsBellEnabled && talk.BellApplicable && talk.AutoBell)
            {
                _bellService.Play(_optionsService.Options.BellVolumePercent);
            }
        }
    }
        
    private void SetDurationStringAttributes(TalkScheduleItem? talk)
    {
        var properties = DurationStringGeneration.Get(_optionsService.GetAdaptiveMode(), talk);

        Duration1String = properties.Duration1String;
        Duration1Tooltip = properties.Duration1Tooltip;
        Duration1Colour = properties.Duration1Colour;

        Duration2String = properties.Duration2String;
        Duration2Tooltip = properties.Duration2Tooltip;
        Duration2Colour = properties.Duration2Colour;

        Duration3String = properties.Duration3String;
        Duration3Tooltip = properties.Duration3Tooltip;
        Duration3Colour = properties.Duration3Colour;
            
        OnPropertyChanged(nameof(Duration1Colour));
        OnPropertyChanged(nameof(Duration2Colour));
        OnPropertyChanged(nameof(Duration3Colour));

        OnPropertyChanged(nameof(Duration1Tooltip));
        OnPropertyChanged(nameof(Duration2Tooltip));
        OnPropertyChanged(nameof(Duration3Tooltip));
    }

    private void AdjustTalkTimeForThisSession()
    {
        var talk = GetCurrentTalk();
        if (talk?.Editable == true)
        {
            var modifiedDuration = TimeSpan.FromSeconds(TargetSeconds);

            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug(
                    "Talk timer ({TalkName}) adjusted for this session. Modified duration = {ModifiedDuration}",
                    talk.Name,
                    modifiedDuration);
            }

            if (IsManualMode)
            {
                talk.OriginalDuration = modifiedDuration;
            }
            else
            {
                talk.ModifiedDuration = modifiedDuration;
            }
                
            SetDurationStringAttributes(talk);

            _scheduleService.SetModifiedDuration(talk.Id, talk.ModifiedDuration);
        }
    }

    private void CloseCountdownWindow()
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Sending StopCountDownMessage");
        }

        WeakReferenceMessenger.Default.Send(new StopCountDownMessage());
    }

    private void OnShowCircuitVisitToggleChanged(object recipient, ShowCircuitVisitToggleChangedMessage message)
    {
        OnPropertyChanged(nameof(ShouldShowCircuitVisitToggle));
    }

    private void OnCountdownWindowStatusChanged(object recipient, CountdownWindowStatusChangedMessage message)
    {
        if (message.Showing)
        {
            _meetingStartTimeFromCountdown = null;
        }
        else
        {
            _meetingStartTimeFromCountdown = DateUtils.GetNearestMinute(_dateTimeService.Now());
        }

        IsCountdownActive = message.Showing;
        OnPropertyChanged(nameof(IsCountdownActive));
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
        return GetCurrentTalk()?.Editable == true;
    }

    private void OnAutoMeetingChanged(object recipient, AutoMeetingChangedMessage message)
    {
        try
        {
            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Meeting schedule changing");
            }

            RefreshSchedule();
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not handle change of meeting schedule";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    private void IncrementDecrementTimerInternal(int mins)
    {
        var newSecs = Math.Max(TargetSeconds + mins, 0);
        if (newSecs <= MaxTimerSecs)
        {
            TargetSeconds = newSecs;
            AdjustTalkTimeForThisSession();
        }
    }

    private void DecrementTimer()
    {
        EventTracker.TrackDecrement(60);
        IncrementDecrementTimerInternal(-60);
    }

    private void DecrementTimer15Secs()
    {
        EventTracker.TrackDecrement(15);
        IncrementDecrementTimerInternal(-15);
    }

    private void DecrementTimer5Mins()
    {
        EventTracker.TrackDecrement(5 * 60);
        IncrementDecrementTimerInternal(-5 * 60);
    }

    private void IncrementTimer()
    {
        EventTracker.TrackIncrement(60);
        IncrementDecrementTimerInternal(60);
    }

    private void IncrementTimer15Secs()
    {
        EventTracker.TrackIncrement(15);
        IncrementDecrementTimerInternal(15);
    }

    private void IncrementTimer5Mins()
    {
        EventTracker.TrackIncrement(5 * 60);
        IncrementDecrementTimerInternal(5 * 60);
    }

    private void OnOperatingModeChanged(object recipient, OperatingModeChangedMessage message)
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Responding to change in operating mode");
        }

        OnPropertyChanged(nameof(IsAutoMode));
        OnPropertyChanged(nameof(ShouldShowCircuitVisitToggle));
        OnPropertyChanged(nameof(IsManualMode));
        OnPropertyChanged(nameof(IsNotManualMode));
        WeakReferenceMessenger.Default.Send(new AutoMeetingChangedMessage());
    }

    private void SelectFirstTalk()
    {
        var talks = _scheduleService.GetTalkScheduleItems().ToArray();
        if (talks.Length > 0)
        {
            TalkId = talks.First().Id;
        }
    }

    private bool IsFirstTalk(int talkId)
    {
        var talks = _scheduleService.GetTalkScheduleItems().ToArray();
        return talks.Length > 0 && talkId == talks.First().Id;
    }

    private TalkScheduleItem? GetPreviousTalk(int talkId)
    {
        var talks = _scheduleService.GetTalkScheduleItems();
            
        TalkScheduleItem? prevTalk = null;
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

    private void NavigateSettings()
    {
        WeakReferenceMessenger.Default.Send(new BeforeNavigateMessage(PageName, SettingsPageViewModel.PageName, null));
        WeakReferenceMessenger.Default.Send(new NavigateMessage(PageName, SettingsPageViewModel.PageName, null));
    }

#pragma warning disable S3168 // "async" methods should not return "void"
    private async void StopTimer()
#pragma warning restore S3168 // "async" methods should not return "void"
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Stopping timer");
        }

        EventTracker.Track(EventName.StoppingTimer);

        IsOvertime = false;

        var talk = GetCurrentTalk();
        if (talk == null)
        {
            Log.Logger.Warning("Could not get current talk!");
            return;
        }

        var msg = new TimerStopMessage(
            TalkId, 
            _timerService.CurrentSecondsElapsed, 
            _optionsService.Options.PersistStudentTime && talk.PersistFinalTimerValue);
            
        _timerService.Stop();
        _isStarting = false;

        StoreTimerStopData();

        TextColor = WhiteBrush;

        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(IsNotRunning));
        OnPropertyChanged(nameof(SettingsHint));

        WeakReferenceMessenger.Default.Send(msg);
        RaiseCanExecuteChanged();

        AutoAdvance();

        if (TalkId == 0)
        {
            // end of the meeting.
            StoreEndOfMeetingData();
            await GenerateTimingReportAsync().ConfigureAwait(false);
        }
        else
        {
            NotifyOfBadTimingIfRequired();
        }
    }

    private void NotifyOfBadTimingIfRequired()
    {
        if (IsAutoMode)
        {
            var overrun = _adaptiveTimerService.CalculateMeetingOverrun(TalkId);
            if (overrun != null)
            {
                _overrunService.NotifyOfBadTiming(overrun.Value);
            }
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
                if (latestVersion != null && latestVersion > VersionDetection.GetCurrentVersion())
                {
                    // there is a new version....
                    IsNewVersionAvailable = true;
                    OnPropertyChanged(nameof(IsNewVersionAvailable));
                }
            });
        }
    }

    private void DisplayNewVersionPage()
    {
        var psi = new ProcessStartInfo
        {
            FileName = VersionDetection.LatestReleaseUrl,
            UseShellExecute = true
        };

        Process.Start(psi);
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
        EventTracker.Track(EventName.LaunchHelp);

        var psi = new ProcessStartInfo
        {
            FileName = "https://github.com/AntonyCorbett/OnlyT/wiki",
            UseShellExecute = true
        };

        Process.Start(psi);
    }

    private void BellToggle()
    {
        var talk = GetCurrentTalk();
        if (talk != null)
        {
            if (_isOvertime)
            {
                // manually sound the bell
                _bellService.Play(_optionsService.Options.BellVolumePercent);
            }
            else
            {
                // toggle state
                talk.AutoBell = !talk.AutoBell;
                OnPropertyChanged(nameof(BellColour));
                OnPropertyChanged(nameof(BellTooltip));
            }
        }
    }

    private void HandleTimerStartStopFromApi(object? sender, OnlyT.EventArgs.TimerStartStopEventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            // always on UI thread to prevent synchronisation issues.
            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Handling timer control from API");
            }

            CheckTalkExists(e.TalkId);

            var success = TalkId == e.TalkId || IsNotRunning;
                
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
                                // fire and forget
                                StopTimer();
                            }

                            break;

                        default:
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
        _ = _scheduleService.GetTalkScheduleItem(talkId) 
            ?? throw new WebServerException(WebServerErrorCode.TimerDoesNotExist);
    }

    private void RaiseCanExecuteIncrementDecrementChanged()
    {
        IncrementTimerCommand?.NotifyCanExecuteChanged();
        IncrementTimer5Command?.NotifyCanExecuteChanged();
        IncrementTimer15Command?.NotifyCanExecuteChanged();

        DecrementTimerCommand?.NotifyCanExecuteChanged();
        DecrementTimer5Command?.NotifyCanExecuteChanged();
        DecrementTimer15Command?.NotifyCanExecuteChanged();
    }

    private async Task GenerateTimingReportAsync()
    {
        if (_optionsService.Options.OperatingMode == OperatingMode.Automatic &&
            _optionsService.Options.GenerateTimingReports)
        {
            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Generating timer report");
            }

            if (!InShrinkMode)
            {
                _snackbarService.EnqueueWithOk(Properties.Resources.ANALYSING_REPORT_DATA);
            }

            _timingDataService.Save();

            var reportPath = await TimingReportGeneration.ExecuteAsync(
                _timingDataService, 
                _dateTimeService,
                _queryWeekendService,
                _optionsService.Options.WeekendIncludesFriday,
                _commandLineService.OptionsIdentifier).ConfigureAwait(false);

            if (!InShrinkMode)
            {
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
    }

    private static void LaunchPdf(string pdf)
    {
        if (File.Exists(pdf))
        {
            var psi = new ProcessStartInfo
            {
                FileName = pdf,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
    }

    private void OnAutoBellSettingChanged(object recipient, AutoBellSettingChangedMessage message)
    {
        var talks = _scheduleService.GetTalkScheduleItems();
        var autoBell = _optionsService.Options.AutoBell;

        foreach (var talk in talks)
        {
            if (talk.AutoBell != autoBell)
            {
                talk.AutoBell = autoBell;
            }
        }

        OnPropertyChanged(nameof(BellColour));
        OnPropertyChanged(nameof(BellTooltip));
    }

    private void SetSecondsRemaining(int value)
    {
        if (_secondsRemaining != value)
        {
            _secondsRemaining = value;
            OnPropertyChanged(nameof(CurrentTimerValueString));
        }
    }

    private void OnWindowSizeChanged(object recipient, MainWindowSizeChangedMessage message)
    {
        _isShrunk = message.IsShrunk;

        OnPropertyChanged(nameof(InShrinkMode));
        OnPropertyChanged(nameof(NotInShrinkMode));
        OnPropertyChanged(nameof(StartStopButtonRowSpan));
        OnPropertyChanged(nameof(StartStopButtonHeight));
        OnPropertyChanged(nameof(TimeDisplayColumnSpan));
        OnPropertyChanged(nameof(ShowUpDownButton));
    }

    private void OnRefreshSchedule(object recipient, RefreshScheduleMessage message)
    {
        if (ShouldRefreshMidWeekSchedule())
        {
            // for one reason or another we have been unable to download the 
            // auto schedule for the midweek mtg, so try again...
            RefreshSchedule();
        }
    }

    private bool ShouldRefreshMidWeekSchedule()
    {
        return !IsRunning &&
               _optionsService.Options.OperatingMode == OperatingMode.Automatic &&
               _optionsService.Options.MidWeekOrWeekend == MidWeekOrWeekend.MidWeek &&
               TalkId == (int)TalkTypesAutoMode.OpeningComments && 
               !_scheduleService.SuccessGettingAutoFeedForMidWeekMtg();
    }

    private void RefreshSchedule()
    {
        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Refreshing schedule");
        }

        _scheduleService.Reset();
        TalkId = 0;
        OnPropertyChanged(nameof(Talks));
        SelectFirstTalk();
    }

    private void CloseApp()
    {
        // used only in "shrink" mode
        Application.Current.Shutdown();
    }

    private void ExpandFromShrink()
    {
        WeakReferenceMessenger.Default.Send(new ExpandFromShrinkMessage());
    }
}