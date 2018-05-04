namespace OnlyT.Services.Timer
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Models;
    using Options;
    using TalkSchedule;

    /// <summary>
    /// Used in Automatic mode to automatically adapt talk timings according to
    /// the start time of the meeting and the start time of the talk. See also
    /// TalkScheduleAuto
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class AdaptiveTimerService : IAdaptiveTimerService
    {
        private static readonly int LargestDeviationMinutes = 15;

        private static readonly int SmallestDeviationSecs = 15;

        private readonly IOptionsService _optionsService;

        private readonly ITalkScheduleService _scheduleService;

        private DateTime? _meetingStartTimeUtc;

        public AdaptiveTimerService(IOptionsService optionsService, ITalkScheduleService scheduleService)
        {
            _optionsService = optionsService;
            _scheduleService = scheduleService;
        }

        /// <summary>
        /// Calculates the adapted talk duration for specified talk id
        /// </summary>
        /// <param name="itemId">Talk Id</param>
        /// <returns>Adapted time (or null if time is not adapted)</returns>
        public TimeSpan? CalculateAdaptedDuration(int itemId)
        {
            TalkScheduleItem talk = _scheduleService.GetTalkScheduleItem(itemId);
            if (talk != null)
            {
                EnsureMeetingStartTimeIsSet(talk);

                if (_meetingStartTimeUtc != null)
                {
                    AdaptiveMode adaptiveMode = GetAdaptiveMode();
                    if (adaptiveMode != AdaptiveMode.None)
                    {
                        if (talk.AllowAdaptive)
                        {
                            DateTime talkPlannedStartTime = CalculatePlannedStartTimeOfItem(talk);
                            DateTime talkActualStartTime = DateTime.UtcNow;
                            TimeSpan deviation = talkActualStartTime - talkPlannedStartTime;

                            if (DeviationWithinRange(deviation))
                            {
                                if (adaptiveMode == AdaptiveMode.TwoWay || talkPlannedStartTime < talkActualStartTime)
                                {
                                    TimeSpan remainingAdaptiveTime = CalculateRemainingAdaptiveTime(talk);

                                    double fractionToApplyToThisTalk =
                                       talk.GetDurationSeconds() / remainingAdaptiveTime.TotalSeconds;

                                    double secondsToApply = deviation.TotalSeconds * fractionToApplyToThisTalk;
                                    return talk.OriginalDuration.Subtract(TimeSpan.FromSeconds(secondsToApply));
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private void SetMeetingStartUtc(TalkScheduleItem talk)
        {
            if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
            {
                switch (_optionsService.Options.MidWeekOrWeekend)
                {
                    case MidWeekOrWeekend.Weekend:
                        _meetingStartTimeUtc = CalculateWeekendStartTime(talk);
                        break;

                    case MidWeekOrWeekend.MidWeek:
                        _meetingStartTimeUtc = CalculateMidWeekStartTime(talk);
                        break;
                }
            }
        }

        private void EnsureMeetingStartTimeIsSet(TalkScheduleItem talk)
        {
            if (_meetingStartTimeUtc == null)
            {
                SetMeetingStartUtc(talk);
            }
        }

        private bool DeviationWithinRange(TimeSpan deviation)
        {
            return Math.Abs(deviation.TotalSeconds) > SmallestDeviationSecs
                   && Math.Abs(deviation.TotalMinutes) <= LargestDeviationMinutes;
        }

        private AdaptiveMode GetAdaptiveMode()
        {
            AdaptiveMode result = AdaptiveMode.None;

            if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
            {
                switch (_optionsService.Options.MidWeekOrWeekend)
                {
                    case MidWeekOrWeekend.MidWeek:
                        return _optionsService.Options.MidWeekAdaptiveMode;

                    case MidWeekOrWeekend.Weekend:
                        return _optionsService.Options.WeekendAdaptiveMode;
                }
            }

            return result;
        }

        private TimeSpan CalculateRemainingAdaptiveTime(TalkScheduleItem talk)
        {
            TimeSpan result = TimeSpan.Zero;

            var allItems = _scheduleService.GetTalkScheduleItems().Reverse();
            foreach (var item in allItems)
            {
                if (item.AllowAdaptive)
                {
                    result = result.Add(item.ActualDuration);
                }

                if (item == talk)
                {
                    break;
                }
            }

            return result;
        }

        private DateTime CalculatePlannedStartTimeOfItem(TalkScheduleItem talk)
        {
            Debug.Assert(_meetingStartTimeUtc != null, "Meeting start time is null");

            if (_meetingStartTimeUtc != null)
            {
                return _meetingStartTimeUtc.Value.Add(talk.StartOffsetIntoMeeting);
            }

            return DateTime.MinValue;
        }

        private DateTime? CalculateWeekendStartTime(TalkScheduleItem talk)
        {
            DateTime? result = null;
            if (talk.Id == (int)TalkTypesAutoMode.PublicTalk)
            {
                result = GetNearest15MinsBefore(DateTime.UtcNow);
            }

            return result;
        }

        private DateTime? CalculateMidWeekStartTime(TalkScheduleItem talk)
        {
            DateTime? result = null;
            switch (talk.Id)
            {
                case (int)TalkTypesAutoMode.OpeningComments:
                case (int)TalkTypesAutoMode.TreasuresTalk:
                    result = GetNearest15MinsBefore(DateTime.UtcNow);
                    break;
            }

            return result;
        }

        private DateTime GetNearest15MinsBefore(DateTime dateTimeBase)
        {
            DateTime result = dateTimeBase.Date;
            if (dateTimeBase.Minute > 45)
            {
                result = result.AddHours(dateTimeBase.Hour).AddMinutes(45);
            }
            else if (dateTimeBase.Minute > 30)
            {
                result = result.AddHours(dateTimeBase.Hour).AddMinutes(30);
            }
            else if (dateTimeBase.Minute > 15)
            {
                result = result.AddHours(dateTimeBase.Hour).AddMinutes(15);
            }
            else
            {
                result = result.AddHours(dateTimeBase.Hour);
            }

            return result;
        }
    }
}