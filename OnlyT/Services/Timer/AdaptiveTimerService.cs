namespace OnlyT.Services.Timer
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using GalaSoft.MvvmLight.Messaging;
    using Models;
    using OnlyT.Utils;
    using OnlyT.ViewModel.Messages;
    using Options;
    using Serilog;
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

        private DateTime? _meetingStartTimeUtcFromCountdown;

        public AdaptiveTimerService(
            IOptionsService optionsService, 
            ITalkScheduleService scheduleService)
        {
            _optionsService = optionsService;
            _scheduleService = scheduleService;

            Messenger.Default.Register<CountdownWindowStatusChangedMessage>(this, OnCountdownWindowStatusChanged);
        }

        /// <summary>
        /// Calculates the adapted talk duration for specified talk id
        /// </summary>
        /// <param name="itemId">Talk Id</param>
        /// <returns>Adapted time (or null if time is not adapted)</returns>
        public TimeSpan? CalculateAdaptedDuration(int itemId)
        {
            var talk = _scheduleService.GetTalkScheduleItem(itemId);
            if (talk == null)
            {
                return null;
            }

            Log.Logger.Debug($"Calculating adapted talk duration for item {talk.Name}");

            EnsureMeetingStartTimeIsSet(talk);

            if (_meetingStartTimeUtc == null)
            {
                return null;
            }

            var adaptiveMode = GetAdaptiveMode();

            Log.Logger.Debug($"Adaptive mode = {adaptiveMode}");
            if (adaptiveMode == AdaptiveMode.None)
            {
                return null;
            }

            if (!talk.AllowAdaptive)
            {
                return null;
            }

            var talkPlannedStartTime = CalculatePlannedStartTimeOfItem(talk);
            
            var talkActualStartTime = DateTime.UtcNow;
            
            var deviation = talkActualStartTime - talkPlannedStartTime;

            Log.Logger.Debug($"Talk planned start = {talkPlannedStartTime}, actual start ={talkActualStartTime}, deviation={deviation}");

            if (!DeviationWithinRange(deviation))
            {
                return null;
            }

            if (adaptiveMode != AdaptiveMode.TwoWay && talkPlannedStartTime >= talkActualStartTime)
            {
                return null;
            }

            var remainingAdaptiveTime = CalculateRemainingAdaptiveTime(talk);

            Log.Logger.Debug($"Remaining time = {remainingAdaptiveTime}");

            var fractionToApplyToThisTalk =
               talk.GetDurationSeconds() / remainingAdaptiveTime.TotalSeconds;

            Log.Logger.Debug($"Fraction to apply = {fractionToApplyToThisTalk}");

            var secondsToApply = deviation.TotalSeconds * fractionToApplyToThisTalk;

            Log.Logger.Debug($"Seconds to apply = {secondsToApply}");

            return talk.OriginalDuration.Subtract(TimeSpan.FromSeconds(secondsToApply));
        }

        private void SetMeetingStartUtc(TalkScheduleItem talk)
        {
            if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
            {
                Log.Logger.Debug("Setting meeting start time UTC for adaptive timer service");

                DateTime? start = null;

                switch (_optionsService.Options.MidWeekOrWeekend)
                {
                    case MidWeekOrWeekend.Weekend:
                        start = CalculateWeekendStartTime(talk);
                        break;

                    case MidWeekOrWeekend.MidWeek:
                        start = CalculateMidWeekStartTime(talk);
                        break;
                }

                if (start == null)
                {
                    _meetingStartTimeUtc = _meetingStartTimeUtcFromCountdown;
                }
                else if (_meetingStartTimeUtcFromCountdown == null)
                {
                    _meetingStartTimeUtc = start;
                }
                else
                {
                    // prefer the start time as provided by the countdown
                    // so long as it's within a reasonable time frame...
                    var toleranceSeconds = TimeSpan.FromMinutes(10).TotalSeconds;

                    var diff = _meetingStartTimeUtcFromCountdown - start;
                    if (diff != null && Math.Abs(diff.Value.TotalSeconds) < toleranceSeconds)
                    {
                        _meetingStartTimeUtc = _meetingStartTimeUtcFromCountdown;
                    }
                    else
                    {
                        _meetingStartTimeUtc = start;
                    }
                }

                Log.Logger.Debug($"Meeting start time UTC = {_meetingStartTimeUtc}");
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

        private void OnCountdownWindowStatusChanged(CountdownWindowStatusChangedMessage message)
        {
            if (!message.Showing)
            {
                _meetingStartTimeUtcFromCountdown = DateUtils.GetNearestMinute(DateTime.UtcNow);
            }
            else
            {
                _meetingStartTimeUtcFromCountdown = null;
            }
        }
    }
}