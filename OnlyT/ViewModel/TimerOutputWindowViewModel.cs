namespace OnlyT.ViewModel
{
    // ReSharper disable UnusedMember.Global
    using System;
    using System.Windows.Input;
    using System.Windows.Media;
    using AnalogueClock;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using Messages;
    using Services.Options;
    using Utils;

    public class TimerOutputWindowViewModel : ViewModelBase
    {
        private static int _secsPerHour = 60 * 60;
        private readonly IOptionsService _optionsService;
        private bool _applicationClosing;

        public TimerOutputWindowViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;
        
            AnalogueClockColumnWidthPercentage = _optionsService.Options.AnalogueClockWidthPercent;
            ShowTimeOfDayUnderTimer = _optionsService.Options.ShowTimeOfDayUnderTimer;

            // subscriptions...
            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
            Messenger.Default.Register<TimerChangedMessage>(this, OnTimerChanged);
            Messenger.Default.Register<TimerStartMessage>(this, OnTimerStarted);
            Messenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
            Messenger.Default.Register<ClockHourFormatChangedMessage>(this, OnDigitalClockFormatChanged);
            Messenger.Default.Register<AnalogueClockWidthChangedMessage>(this, OnAnalogueClockWidthChanged);
            Messenger.Default.Register<ShowTimeOfDayUnderTimerChangedMessage>(this, OnShowTimeOfDayUnderTimerChanged);
            Messenger.Default.Register<MousePointerInTimerDisplayChangedMessage>(this, OnMousePointerChanged);
        }

        public bool SplitAndFullScrenModeIdentical()
        {
            return _optionsService.Options.AnalogueClockWidthPercent == 100;
        }

        public bool ApplicationClosing => _applicationClosing;

        private void OnShutDown(ShutDownMessage obj)
        {
            _applicationClosing = true;
        }

        private void OnShowTimeOfDayUnderTimerChanged(ShowTimeOfDayUnderTimerChangedMessage message)
        {
            ShowTimeOfDayUnderTimer = _optionsService.Options.ShowTimeOfDayUnderTimer;
        }

        private void OnAnalogueClockWidthChanged(AnalogueClockWidthChangedMessage obj)
        {
            AnalogueClockColumnWidthPercentage = _optionsService.Options.AnalogueClockWidthPercent;
        }

        private void OnDigitalClockFormatChanged(ClockHourFormatChangedMessage obj)
        {
            RaisePropertyChanged(nameof(DigitalTimeFormat24Hours));
            RaisePropertyChanged(nameof(DigitalTimeFormatShowLeadingZero));
            RaisePropertyChanged(nameof(DigitalTimeFormatAMPM));
        }

        private void OnTimerStopped(TimerStopMessage obj)
        {
            IsRunning = false;
            DurationSector = null;
        }

        private double CalcAngleFromTime(DateTime dt) => (dt.Minute + ((double)dt.Second / 60)) / 60 * 360;

        private void OnTimerStarted(TimerStartMessage message)
        {
            TimeString = TimeFormatter.FormatTimerDisplayString(message.CountUp 
                ? 0
                : message.TargetSeconds);

            IsRunning = true;
            InitOverallDurationSector(message.TargetSeconds);
        }

        private void InitOverallDurationSector(int targetSecs)
        {
            if (DurationSector == null && _optionsService.Options.ShowDurationSector)
            {
                // can't display duration sector effectively when >= 1 hr
                if (targetSecs < _secsPerHour) 
                {
                    var now = DateTime.Now;
                    var startAngle = CalcAngleFromTime(now);

                    var endTime = now.AddSeconds(targetSecs);
                    var endAngle = CalcAngleFromTime(endTime);

                    DurationSector = new DurationSector
                    {
                        StartAngle = startAngle,
                        EndAngle = endAngle,
                        CurrentAngle = startAngle,
                        IsOvertime = false
                    };
                }
            }
        }

        private void OnMousePointerChanged(MousePointerInTimerDisplayChangedMessage message)
        {
            RaisePropertyChanged(nameof(MousePointer));
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public Cursor MousePointer =>
            _optionsService.Options.ShowMousePointerInTimerDisplay
                ? Cursors.Arrow
                : Cursors.None;

        private void OnTimerChanged(TimerChangedMessage message)
        {
            if (message.TimerIsRunning)
            {
                TextColor = GreenYellowRedSelector.GetBrushForTimeRemaining(message.RemainingSecs);
                
                TimeString = TimeFormatter.FormatTimerDisplayString(message.CountUp
                    ? message.ElapsedSecs
                    : message.RemainingSecs);

                // if duration of talk is greater than 1hr we only start showing the sector
                // when remaining time is less than 1 hr (for sake of clarity)...
                InitOverallDurationSector(message.RemainingSecs);

                if (DurationSector != null)
                {
                    var now = DateTime.Now;
                    var currentAngle = CalcAngleFromTime(now);

                    // prevent gratuitous updates
                    if (Math.Abs(currentAngle - DurationSector.CurrentAngle) > 0.15)
                    {
                        var d = DurationSector.Clone();
                        d.CurrentAngle = currentAngle;
                        d.IsOvertime = message.RemainingSecs < 0;
                        DurationSector = d;
                    }
                }
            }
        }

        private int _timerColumnWidthPercentage = -1;

        // ReSharper disable once MemberCanBePrivate.Global
        public int TimerColumnWidthPercentage => _timerColumnWidthPercentage;

        private int _analogueClockColumnWidthPercentage = -1;

        // ReSharper disable once MemberCanBePrivate.Global
        public int AnalogueClockColumnWidthPercentage
        {
            get => _analogueClockColumnWidthPercentage;
            set
            {
                if (_analogueClockColumnWidthPercentage != value)
                {
                    _analogueClockColumnWidthPercentage = value;
                    _timerColumnWidthPercentage = 100 - _analogueClockColumnWidthPercentage;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(TimerColumnWidthPercentage));
                }
            }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public bool DigitalTimeFormatShowLeadingZero =>
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZero ||
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZeroAMPM ||
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24LeadingZero;

        // ReSharper disable once MemberCanBePrivate.Global
        public bool DigitalTimeFormat24Hours =>
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24 ||
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24LeadingZero;

        // ReSharper disable once MemberCanBePrivate.Global
        public bool DigitalTimeFormatAMPM =>
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12AMPM ||
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZeroAMPM;

        private string _timeString;

        public string TimeString
        {
            get => _timeString;
            set
            {
                if (_timeString != value)
                {
                    _timeString = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _isRunning;

        // ReSharper disable once MemberCanBePrivate.Global
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    RaisePropertyChanged();
                }
            }
        }

        private Brush _textColor = GreenYellowRedSelector.GetGreenBrush();

        public Brush TextColor
        {
            get => _textColor;
            set
            {
                if (!ReferenceEquals(_textColor, value))
                {
                    _textColor = value;
                    RaisePropertyChanged();
                }
            }
        }

        private DurationSector _durationSector;

        // ReSharper disable once MemberCanBePrivate.Global
        public DurationSector DurationSector
        {
            get => _durationSector;
            set
            {
                if (!ReferenceEquals(_durationSector, value))
                {
                    _durationSector = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _showTimeOfDayUnderTimer;

        // ReSharper disable once MemberCanBePrivate.Global
        public bool ShowTimeOfDayUnderTimer
        {
            get => _showTimeOfDayUnderTimer;
            set
            {
                if (_showTimeOfDayUnderTimer != value)
                {
                    _showTimeOfDayUnderTimer = value;
                    RaisePropertyChanged();
                }
            }
        }

        public FullScreenClockMode FullScreenClockMode => _optionsService.Options.FullScreenClockMode;
    }
}
