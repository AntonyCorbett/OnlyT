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
    using System.Diagnostics;
    using System.Threading.Tasks;

    internal class TimerOutputWindowViewModel : ViewModelBase
    {
        private static int _secsPerHour = 60 * 60;
        private readonly IOptionsService _optionsService;

        public TimerOutputWindowViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;

            AnalogueClockColumnWidthPercentage = _optionsService.Options.AnalogueClockWidthPercent;
            
            // subscriptions...
            Messenger.Default.Register<TimerChangedMessage>(this, OnTimerChanged);
            Messenger.Default.Register<TimerStartMessage>(this, OnTimerStarted);
            Messenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
            Messenger.Default.Register<ClockHourFormatChangedMessage>(this, OnDigitalClockFormatChanged);
            Messenger.Default.Register<AnalogueClockWidthChangedMessage>(this, OnAnalogueClockWidthChanged);
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
        }

        private void OnNavigate(NavigateMessage message)
        {
            if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
            {
                // when the settings page is displayed we ensure that the analogue 
                // clock is shown in its restored state rather than enlarged...
                RunStartTimerAnimation();

                // and that the timer text is shown...
                TimeString = TimeFormatter.FormatTimeRemaining(0);
            }
            else if (message.OriginalPageName.Equals(SettingsPageViewModel.PageName))
            {
                RunStopTimerAnimation();
            }
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

            RunStopTimerAnimation();
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

            RunStartTimerAnimation();
        }

        private void RunStartTimerAnimation()
        {
            RunTimerStartAnimationAutoEnlarge = false;
            RunTimerStartAnimationAutoEnlarge = true;
        }

        private void RunStopTimerAnimation()
        {
            RunTimerStopAnimationAutoEnlarge = false;
            RunTimerStopAnimationAutoEnlarge = true;
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

        private int _timerColumnWidthPercentage;
        public int TimerColumnWidthPercentage
        {
            get => _timerColumnWidthPercentage;
            set
            {
                if (_timerColumnWidthPercentage != value)
                {
                    _timerColumnWidthPercentage = value;
                    TimerColumnWidth = $"{_timerColumnWidthPercentage}*";
                }
            }
        }

        private string _timerColumnWidth;
        public string TimerColumnWidth
        {
            get => _timerColumnWidth;
            set
            {
                if (_timerColumnWidth != value)
                {
                    _timerColumnWidth = value;
                    RaisePropertyChanged();
                }
            }
        }

        private int _analogueClockColumnWidthPercentage;
        private int AnalogueClockColumnWidthPercentage
        {
            get => _analogueClockColumnWidthPercentage;
            set
            {
                if (_analogueClockColumnWidthPercentage != value)
                {
                    _analogueClockColumnWidthPercentage = value;
                    AnalogueClockColumnWidth = $"{_analogueClockColumnWidthPercentage}*";
                    TimerColumnWidthPercentage = 100 - _analogueClockColumnWidthPercentage;
                }
            }
        }

        private string _analogueClockColumnWidth;
        public string AnalogueClockColumnWidth
        {
            get => _analogueClockColumnWidth;
            set
            {
                if (_analogueClockColumnWidth != value)
                {
                    _analogueClockColumnWidth = value;
                    RaisePropertyChanged();
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

        private bool _runTimerStartAnimationAutoEnlarge;
        public bool RunTimerStartAnimationAutoEnlarge
        {
            get => _runTimerStartAnimationAutoEnlarge;
            set
            {
                if (_runTimerStartAnimationAutoEnlarge != value)
                {
                    _runTimerStartAnimationAutoEnlarge = value;
                    RaisePropertyChanged();
                }
            }
        }

        private bool _runTimerStopAnimationAutoEnlarge;
        public bool RunTimerStopAnimationAutoEnlarge
        {
            get => _runTimerStopAnimationAutoEnlarge;
            set
            {
                if (_runTimerStopAnimationAutoEnlarge != value)
                {
                    _runTimerStopAnimationAutoEnlarge = value;
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
    }
}
