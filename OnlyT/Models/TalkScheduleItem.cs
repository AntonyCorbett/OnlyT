namespace OnlyT.Models
{
    using System;
    using System.Windows.Media;
    using GalaSoft.MvvmLight;
    using Services.TalkSchedule;
    using Utils;

    /// <summary>
    /// Represents a talk in the meeting schedule
    /// </summary>
    public class TalkScheduleItem : ObservableObject
    {
        /// <summary>
        /// Manually modified duration (after user modification)
        /// </summary>
        private TimeSpan? _modifiedDuration;
        private bool? _originalBell;
        private bool _bell;
        private int? _completedSeconds;

        public TalkScheduleItem()
        {
        }

        public TalkScheduleItem(TalkTypesAutoMode tt)
        {
            Id = (int)tt;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string MeetingSectionNameInternal { get; set; }

        public string MeetingSectionNameLocalised { get; set; }

        // ReSharper disable once UnusedMember.Global
        public string NameIncludingDuration => 
            $"{TimeFormatter.FormatTimerDisplayString((int)OriginalDuration.TotalSeconds)} {Name}";

        public string OriginalDurationAsString =>
            TimeFormatter.FormatTimerDisplayString((int)OriginalDuration.TotalSeconds);

        public bool? CountUp { get; set; }

        /// <summary>
        /// Gets or sets the duration for which the timer ran (or null if not run yet)
        /// </summary>
        public int? CompletedTimeSecs
        {
            get => _completedSeconds;
            set
            {
                if (_completedSeconds != value)
                {
                    _completedSeconds = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(OvertimeString));
                    RaisePropertyChanged(nameof(OvertimeBrush));
                }
            }
        }

        public string OvertimeString
        {
            get
            {
                if (CompletedTimeSecs == null || CompletedTimeSecs == 0)
                {
                    return null;
                }

                var overtimeSecs = (int)(ActualDuration.TotalSeconds - CompletedTimeSecs ?? 0);
                var timeString = TimeFormatter.FormatTimerDisplayString(Math.Abs(overtimeSecs));
                return overtimeSecs >= 0
                    ? $"-{timeString}"
                    : $"+{timeString}";
            }
        }

        public Brush OvertimeBrush
        {
            get
            {
                var overtimeSecs = ActualDuration.TotalSeconds - CompletedTimeSecs ?? 0;
                if (overtimeSecs >= 0)
                {
                    return Brushes.Green;
                }
                
                return Brushes.Red;
            }
        }

        /// <summary>
        /// Gets or sets the original duration (before any user modification)
        /// </summary>
        public TimeSpan OriginalDuration { get; set; }

        public TimeSpan? ModifiedDuration
        {
            get => _modifiedDuration;
            set
            {
                if (_modifiedDuration != value)
                {
                    _modifiedDuration = value != null && value == OriginalDuration ? null : value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the adapted duration (following adaptive timer service calculation)
        /// </summary>
        public TimeSpan? AdaptedDuration { get; set; }

        /// <summary>
        /// Gets the actual duration used (may be original, manually adjusted or adapted)
        /// </summary>
        public TimeSpan ActualDuration 
        {
            get
            {
                if (AdaptedDuration != null)
                {
                    return AdaptedDuration.Value;
                }
                
                if (ModifiedDuration != null)
                {
                    return ModifiedDuration.Value;
                }
                
                return OriginalDuration;
            }
        }

        public TimeSpan StartOffsetIntoMeeting { get; set; }

        public bool AllowAdaptive { get; set; }

        public bool PersistFinalTimerValue { get; set; }

        // can the timer be modified manually?
        public bool Editable { get; set; } 

        public bool OriginalBell
        {
            get => _originalBell ?? false;
            private set
            {
                if (_originalBell == null)
                {
                    _originalBell = value;
                }
            }
        }
        
        // should a bell be sounded at time-up?
        public bool Bell
        {
            get => _bell;
            set
            {
                if (_bell != value)
                {
                    _bell = value;
                    OriginalBell = value;
                }
            }
        }

        public bool IsStudentTalk { get; set; }

        public int GetDurationSeconds()
        {
            return (int)ActualDuration.TotalSeconds;
        }
    }
}
