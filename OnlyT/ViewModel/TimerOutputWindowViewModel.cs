using System;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.AnalogueClock;
using OnlyT.Services.Options;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
    internal class TimerOutputWindowViewModel : ViewModelBase
    {
        private static int _secsPerHour = 60 * 60;
        private readonly IOptionsService _optionsService;

        public TimerOutputWindowViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;

            AnalogueClockColumnWidthPercentage = _optionsService.Options.AnalogueClockWidthPercent;
            ShowTimeOfDayUnderTimer = _optionsService.Options.ShowTimeOfDayUnderTimer;

            // subscriptions...
            Messenger.Default.Register<TimerChangedMessage>(this, OnTimerChanged);
            Messenger.Default.Register<TimerStartMessage>(this, OnTimerStarted);
            Messenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
            Messenger.Default.Register<ClockHourFormatChangedMessage>(this, OnDigitalClockFormatChanged);
            Messenger.Default.Register<AnalogueClockWidthChangedMessage>(this, OnAnalogueClockWidthChanged);
            Messenger.Default.Register<ShowTimeOfDayUnderTimerChangedMessage>(this, OnShowTimeOfDayUnderTimerChangedMessage);
        }

        private void OnShowTimeOfDayUnderTimerChangedMessage(ShowTimeOfDayUnderTimerChangedMessage message)
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
        }

        private void OnTimerStopped(TimerStopMessage obj)
        {
            IsRunning = false;
            DurationSector = null;
        }

        private double CalcAngleFromTime(DateTime dt)
        {
            return (dt.Minute + (double)dt.Second / 60) / 60 * 360;
        }

        private void OnTimerStarted(TimerStartMessage message)
        {
            TimeString = TimeFormatter.FormatTimeRemaining(message.TargetSeconds);
            IsRunning = true;
            InitOverallDurationSector(message.TargetSeconds);
        }

        private void InitOverallDurationSector(int targetSecs)
        {
            if (DurationSector == null)
            {
                if (targetSecs < _secsPerHour) // can't display duration sector effectively when >= 1 hr
                {
                    DateTime now = DateTime.Now;
                    double startAngle = CalcAngleFromTime(now);

                    DateTime endTime = now.AddSeconds(targetSecs);
                    double endAngle = CalcAngleFromTime(endTime);

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

        private void OnTimerChanged(TimerChangedMessage message)
        {
            if (message.TimerIsRunning)
            {
                TextColor = GreenYellowRedSelector.GetBrushForTimeRemaining(message.RemainingSecs);
                TimeString = TimeFormatter.FormatTimeRemaining(message.RemainingSecs);

                // if duration of talk is greater than 1hr we only start showing the sector
                // when remaining time is less than 1 hr (for sake of clarity)...
                InitOverallDurationSector(message.RemainingSecs);

                if (DurationSector != null)
                {
                    DateTime now = DateTime.Now;
                    var currentAngle = CalcAngleFromTime(now);
                    if (Math.Abs(currentAngle - DurationSector.CurrentAngle) > 0.15) // prevent gratuitous updates
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
        public int TimerColumnWidthPercentage => _timerColumnWidthPercentage;

        private int _analogueClockColumnWidthPercentage = -1;
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

        public bool DigitalTimeFormatShowLeadingZero =>
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZero ||
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24LeadingZero;

        public bool DigitalTimeFormat24Hours =>
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24 ||
           _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24LeadingZero;

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
