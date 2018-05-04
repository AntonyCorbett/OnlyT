namespace OnlyT.Models
{
    using System;
    using Services.TalkSchedule;
    using Utils;

    /// <summary>
    /// Represents a talk in the meeting schedule
    /// </summary>
    public class TalkScheduleItem
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // ReSharper disable once UnusedMember.Global
        public string NameIncludingDuration => $"{TimeFormatter.FormatTimerDisplayString((int)OriginalDuration.TotalSeconds)} {Name}";

        public bool? CountUp { get; set; }

        /// <summary>
        /// The duration for which the timer ran (or null if not run yet)
        /// </summary>
        public int? CompletedTimeSecs { get; set; }

        /// <summary>
        /// Original duration (before any user modification)
        /// </summary>
        public TimeSpan OriginalDuration { get; set; }

        /// <summary>
        /// Manually modified duration (after user modification)
        /// </summary>
        private TimeSpan? _modifiedDuration;

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
        /// Adapted duration (following adaptive timer service calculation)
        /// </summary>
        public TimeSpan? AdaptedDuration { get; set; }

        /// <summary>
        /// Actual duration used (may be original, manually adjusted or adapted)
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

        public bool Editable { get; set; }  // can the timer be modified manually?

        private bool? _originalBell;

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

        private bool _bell;
        
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
        
        public TalkScheduleItem()
        {
        }

        public TalkScheduleItem(TalkTypesAutoMode tt)
        {
            Id = (int)tt;
        }

        public int GetDurationSeconds()
        {
            return (int)ActualDuration.TotalSeconds;
        }
    }
}
