using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows.Media;
using OnlyT.Services.TalkSchedule;
using OnlyT.Utils;

namespace OnlyT.Models;

/// <summary>
/// Represents a talk in the meeting schedule
/// </summary>
public class TalkScheduleItem : ObservableObject
{
    public const int DefaultClosingSecs = 30;

    /// <summary>
    /// Manually modified duration (after user modification)
    /// </summary>
    private TimeSpan? _modifiedDuration;
    private int? _completedSeconds;
    private bool _autoBell;
    private bool? _originalAutoBell;

    public TalkScheduleItem(TalkTypesAutoMode tt, string name, string meetingSectionNameInternal, string meetingSectionNameLocalised)
        : this((int)tt, name, meetingSectionNameInternal, meetingSectionNameLocalised)
    {
    }

    public TalkScheduleItem(int talkId, string name, string meetingSectionNameInternal, string meetingSectionNameLocalised)
    {
        Id = talkId;
        Name = name;
        MeetingSectionNameInternal = meetingSectionNameInternal;
        MeetingSectionNameLocalised = meetingSectionNameLocalised;
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

    public string OriginalOrModifiedDurationAsString =>
        TimeFormatter.FormatTimerDisplayString(ModifiedDuration == null 
            ? (int)OriginalDuration.TotalSeconds
            : (int)ModifiedDuration.Value.TotalSeconds);

    public bool? CountUp { get; set; }

    public int ClosingSecs { get; set; } = DefaultClosingSecs;

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
                OnPropertyChanged();
                OnPropertyChanged(nameof(OvertimeString));
                OnPropertyChanged(nameof(OvertimeBrush));
            }
        }
    }

    public string? OvertimeString
    {
        get
        {
            if (CompletedTimeSecs == null || CompletedTimeSecs == 0)
            {
                return null;
            }

            var overtimeSecs = (int)(ActualDuration.TotalSeconds - CompletedTimeSecs);
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
                OnPropertyChanged();
                OnPropertyChanged(nameof(OriginalOrModifiedDurationAsString));
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

    /// <summary>
    /// Gets the planned duration used (may be original or manually adjusted)
    /// </summary>
    public TimeSpan PlannedDuration => ModifiedDuration ?? OriginalDuration;

    public TimeSpan StartOffsetIntoMeeting { get; set; }

    public bool AllowAdaptive { get; set; }

    public bool PersistFinalTimerValue { get; set; }

    // can the timer be modified manually?
    public bool Editable { get; set; }

    public bool BellApplicable { get; set; }

    // should a bell be sounded at time-up?
    public bool AutoBell
    {
        get => _autoBell;
        set
        {
            if (_autoBell != value)
            {
                _autoBell = value;
                OriginalAutoBell = value;
            }
        }
    }

    public bool OriginalAutoBell
    {
        get => _originalAutoBell ?? false;
        private set => _originalAutoBell ??= value;
    }

    public bool IsStudentTalk { get; set; }

    public int GetPlannedDurationSeconds()
    {
        return (int)PlannedDuration.TotalSeconds;
    }
}