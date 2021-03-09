using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace OnlyT.ViewModel
{
    // ReSharper disable UnusedMember.Global
    using System;
    using System.Windows.Input;
    using System.Windows.Media;
    using AnalogueClock;
    using Messages;
    using OnlyT.Common.Services.DateTime;
    using Services.Options;
    using Utils;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class TimerOutputWindowViewModel : ObservableObject
    {
        private static readonly int _secsPerHour = 60 * 60;
        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;

        private int _analogueClockColumnWidthPercentage = -1;
        private string _timeString;
        private bool _isRunning;
        private Brush _textColor = GreenYellowRedSelector.GetGreenBrush();
        private DurationSector _durationSector;
        private bool _showTimeOfDayUnderTimer;
        private bool _windowedOperation;

        public TimerOutputWindowViewModel(
            IOptionsService optionsService,
            IDateTimeService dateTimeService)
        {
            _optionsService = optionsService;
            _dateTimeService = dateTimeService;

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
            Messenger.Default.Register<TimerFrameChangedMessage>(this, OnTimerFrameChanged);
            Messenger.Default.Register<ClockIsFlatChangedMessage>(this, OnClockIsFlatChanged);
        }

        public int BorderThickness => !WindowedOperation && _optionsService.Options.TimerFrame ? 3 : 0;
        
        public int TimerBorderThickness => !WindowedOperation && _optionsService.Options.ClockTimerFrame ? 3 : 0;

        public int TimerColumnWidthPercentage { get; private set; } = -1;

        public bool ApplicationClosing { get; private set; }

        public bool ClockIsFlat => WindowedOperation || _optionsService.Options.ClockIsFlat;

        public bool ShowTimeOfDayUnderTimer
        {
            get => _showTimeOfDayUnderTimer;
            set
            {
                if (_showTimeOfDayUnderTimer != value)
                {
                    _showTimeOfDayUnderTimer = value;
                    OnPropertyChanged();
                }
            }
        }

        public FullScreenClockMode FullScreenClockMode => _optionsService.Options.FullScreenClockMode;

        public Cursor MousePointer =>
            _optionsService.Options.ShowMousePointerInTimerDisplay
                ? Cursors.Arrow
                : Cursors.None;

        public string TimeString
        {
            get => _timeString;
            set
            {
                if (_timeString != value)
                {
                    _timeString = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public DurationSector DurationSector
        {
            get => _durationSector;
            set
            {
                if (!ReferenceEquals(_durationSector, value))
                {
                    _durationSector = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush TextColor
        {
            get => _textColor;
            set
            {
                if (!ReferenceEquals(_textColor, value))
                {
                    _textColor = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public int AnalogueClockColumnWidthPercentage
        {
            get => _analogueClockColumnWidthPercentage;
            set
            {
                if (_analogueClockColumnWidthPercentage != value)
                {
                    _analogueClockColumnWidthPercentage = value;
                    TimerColumnWidthPercentage = 100 - _analogueClockColumnWidthPercentage;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TimerColumnWidthPercentage));
                }
            }
        }
        
        public bool DigitalTimeFormatShowLeadingZero =>
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZero ||
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZeroAMPM ||
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24LeadingZero;
        
        public bool DigitalTimeFormat24Hours =>
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24 ||
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24LeadingZero;
        
        public bool DigitalTimeFormatAMPM =>
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12AMPM ||
            _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZeroAMPM;

        public bool DigitalTimeShowSeconds => _optionsService.Options.ShowDigitalSeconds;

        public bool UseTimerBackgroundGradient => !WindowedOperation && _optionsService.Options.ShowBackgroundOnTimer;

        public bool UseClockBackgroundGradient => !WindowedOperation && _optionsService.Options.ShowBackgroundOnClock;

        public bool WindowedOperation
        {
            get => _windowedOperation;
            set
            {
                if (_windowedOperation != value)
                {
                    _windowedOperation = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(UseTimerBackgroundGradient));
                    OnPropertyChanged(nameof(UseClockBackgroundGradient));
                    OnPropertyChanged(nameof(BorderThickness));
                    OnPropertyChanged(nameof(TimerBorderThickness));
                    OnPropertyChanged(nameof(ClockIsFlat));
                }
            }
        }

        public bool SplitAndFullScreenModeIdentical()
        {
            return _optionsService.Options.AnalogueClockWidthPercent == 100;
        }

        private void OnShutDown(ShutDownMessage obj)
        {
            ApplicationClosing = true;
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
            OnPropertyChanged(nameof(DigitalTimeFormat24Hours));
            OnPropertyChanged(nameof(DigitalTimeFormatShowLeadingZero));
            OnPropertyChanged(nameof(DigitalTimeFormatAMPM));
            OnPropertyChanged(nameof(DigitalTimeShowSeconds));
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
            InitOverallDurationSector(0, message.TargetSeconds);
        }

        private void InitOverallDurationSector(int elapsedSecs, int remainingSecs)
        {
            if (DurationSector == null && _optionsService.Options.ShowDurationSector)
            {
                // can't display duration sector effectively when >= 1 hr
                if (remainingSecs < _secsPerHour) 
                {
                    var now = _dateTimeService.Now();
                    var startAngle = CalcAngleFromTime(now);

                    var endTime = now.AddSeconds(remainingSecs);
                    var endAngle = CalcAngleFromTime(endTime);

                    DurationSector = new DurationSector
                    {
                        StartAngle = startAngle,
                        EndAngle = endAngle,
                        CurrentAngle = startAngle,
                        IsOvertime = false,
                        ShowElapsedSector = (elapsedSecs + remainingSecs) < _secsPerHour
                    };
                }
            }
        }

        private void OnMousePointerChanged(MousePointerInTimerDisplayChangedMessage message)
        {
            OnPropertyChanged(nameof(MousePointer));
        }

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
                InitOverallDurationSector(message.ElapsedSecs, message.RemainingSecs);

                if (DurationSector != null)
                {
                    var now = _dateTimeService.Now();
                    var currentAngle = CalcAngleFromTime(now);

                    // prevent gratuitous updates
                    if (Math.Abs(currentAngle - DurationSector.CurrentAngle) > 0.15)
                    {
                        var d = DurationSector.Clone();
                        d.CurrentAngle = currentAngle;
                        d.IsOvertime = message.RemainingSecs < 0;
                        d.ShowElapsedSector = (message.ElapsedSecs + message.RemainingSecs) <= _secsPerHour;

                        DurationSector = d;
                    }
                }
            }
        }

        private void OnTimerFrameChanged(TimerFrameChangedMessage msg)
        {
            OnPropertyChanged(nameof(BorderThickness));
            OnPropertyChanged(nameof(TimerBorderThickness));
            OnPropertyChanged(nameof(UseTimerBackgroundGradient));
            OnPropertyChanged(nameof(UseClockBackgroundGradient));
        }

        private void OnClockIsFlatChanged(ClockIsFlatChangedMessage msg)
        {
            OnPropertyChanged(nameof(ClockIsFlat));
        }
    }
}
