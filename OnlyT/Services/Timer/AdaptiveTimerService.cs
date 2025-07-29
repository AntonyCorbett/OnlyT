﻿using System;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.Messaging;
using OnlyT.Models;
using OnlyT.Common.Services.DateTime;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;
using OnlyT.Services.Options;
using Serilog;
using OnlyT.Services.TalkSchedule;
using Serilog.Events;

namespace OnlyT.Services.Timer;

/// <summary>
/// Used in Automatic mode to automatically adapt talk timings according to
/// the start time of the meeting and the start time of the talk. See also
/// TalkScheduleAuto
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class AdaptiveTimerService : IAdaptiveTimerService
{
    private static readonly int LargestDeviationMinutes = 15;

    private static readonly int SmallestDeviationSecs = 15;

    private readonly IOptionsService _optionsService;

    private readonly ITalkScheduleService _scheduleService;

    private readonly IDateTimeService _dateTimeService;

    private DateTime? _meetingStartTimeUtc;

    private DateTime? _meetingStartTimeUtcFromCountdown;

    public AdaptiveTimerService(
        IOptionsService optionsService, 
        ITalkScheduleService scheduleService,
        IDateTimeService dateTimeService)
    {
        _optionsService = optionsService;
        _scheduleService = scheduleService;
        _dateTimeService = dateTimeService;

        WeakReferenceMessenger.Default.Register<CountdownWindowStatusChangedMessage>(this, OnCountdownWindowStatusChanged);
    }

    public void SetMeetingStartTimeForTesting(DateTime startTime)
    {
        _meetingStartTimeUtc = startTime;
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

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Calculating adapted talk duration for item {TalkName}", talk.Name);
        }

        EnsureMeetingStartTimeIsSet(talk);

        if (_meetingStartTimeUtc == null)
        {
            return null;
        }

        var adaptiveMode = _optionsService.GetAdaptiveMode();

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Adaptive mode = {AdaptiveMode}", adaptiveMode);
        }
        
        if (adaptiveMode == AdaptiveMode.None || !talk.AllowAdaptive)
        {
            return null;
        }

        var mtgEnd = GetPlannedMeetingEnd();
        var totalTimeRemaining = mtgEnd - _dateTimeService.UtcNow();
        var remainingProgramTimeRequired = GetRemainingProgramTimeRequired(talk);

        var deviation = totalTimeRemaining - remainingProgramTimeRequired;

        if (!IsDeviationSignificant(deviation))
        {
            return null;
        }

        if (adaptiveMode != AdaptiveMode.TwoWay && totalTimeRemaining >= remainingProgramTimeRequired)
        {
            // there is time in hand and we don't want to adaptively increase talk durations.
            return null;
        }

        var remainingAdaptiveTime = CalculateRemainingAdaptiveTimerValues(talk);

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Remaining time = {RemainingTime}", remainingAdaptiveTime);
        }

        var fractionToApplyToThisTalk =
            talk.GetPlannedDurationSeconds() / remainingAdaptiveTime.TotalSeconds;

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Fraction to apply = {FractionToApply:F2}", fractionToApplyToThisTalk);
        }

        var secondsToApply = deviation.TotalSeconds * fractionToApplyToThisTalk;

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Seconds to add = {SecondsToAdd}", secondsToApply);
        }

        return talk.ActualDuration.Add(TimeSpan.FromSeconds(secondsToApply));
    }
    
    public TimeSpan? CalculateMeetingOverrun(int talkId)
    {
        var talk = _scheduleService.GetTalkScheduleItem(talkId);
        if (talk == null)
        {
            return null;
        }

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Calculating meeting overrun");
        }

        EnsureMeetingStartTimeIsSet(talk);

        if (_meetingStartTimeUtc == null)
        {
            return null;
        }

        var adaptiveMode = _optionsService.GetAdaptiveMode();

        if (Log.IsEnabled(LogEventLevel.Debug))
        {
            Log.Logger.Debug("Adaptive mode = {AdaptiveMode}", adaptiveMode);
        }

        if (adaptiveMode == AdaptiveMode.None)
        {
            return null;
        }

        var mtgEnd = GetPlannedMeetingEnd();
        var totalTimeRemaining = mtgEnd - _dateTimeService.UtcNow();
        var remainingProgramTimeRequired = GetRemainingProgramTimeRequired(talk);

        var talkType = (TalkTypesAutoMode)talkId;

        switch (talkType)
        {
            case TalkTypesAutoMode.LivingPart1:
                // skip 3.5 mins for "interval" song
                remainingProgramTimeRequired += TimeSpan.FromMinutes(3.5);
                break;

            case TalkTypesAutoMode.Watchtower:
                // skip 5 mins for "interval" song
                remainingProgramTimeRequired += TimeSpan.FromMinutes(5);
                break;
        }

        var deviation = totalTimeRemaining - remainingProgramTimeRequired;

        if (!IsOverrunSignificant(deviation))
        {
            return null;
        }

        return deviation;
    }

    private static bool IsOverrunSignificant(TimeSpan deviation)
    {
        var mins = Math.Abs(deviation.TotalMinutes);

        // only report over/underrun if between 2 and 20 mins (anything above 20 mins
        // is likely an error).
        return mins >= 2 && mins <= 20;
    }

    private TimeSpan GetRemainingProgramTimeRequired(TalkScheduleItem talk)
    {
        var result = TimeSpan.Zero;

        var started = false;
        var items = _scheduleService.GetTalkScheduleItems().ToArray();
        var lastItem = items.Last();

        for (var n = 0; n < items.Length; ++n)
        {
            var item = items[n];

            if (item == talk)
            {
                started = true;
            }

            if (started)
            {
                result += item.PlannedDuration;

                if (item != lastItem)
                {
                    result += GetTimeForChangeover(item, items[n + 1]);
                }
            }
        }

        return result;
    }

    private static TimeSpan GetTimeForChangeover(TalkScheduleItem item1, TalkScheduleItem item2)
    {
        var endItem1 = item1.StartOffsetIntoMeeting.Add(item1.OriginalDuration);
        return item2.StartOffsetIntoMeeting - endItem1;
    }

    private DateTime GetPlannedMeetingEnd()
    {
        Debug.Assert(_meetingStartTimeUtc != null, "_meetingStartTimeUtc != null");

        var lastItem = _scheduleService.GetTalkScheduleItems().Last();
        return _meetingStartTimeUtc.Value.Add(lastItem.StartOffsetIntoMeeting + lastItem.OriginalDuration);
    }

    private void SetMeetingStartUtc(TalkScheduleItem talk)
    {
        if (_optionsService.Options.OperatingMode == OperatingMode.Automatic)
        {
            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Setting meeting start time UTC for adaptive timer service");
            }

            DateTime? start = null;

            switch (_optionsService.Options.MidWeekOrWeekend)
            {
                case MidWeekOrWeekend.Weekend:
                    start = CalculateWeekendStartTime(talk);
                    break;

                case MidWeekOrWeekend.MidWeek:
                    start = CalculateMidWeekStartTime(talk);
                    break;

                default:
                    // leave start = null
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

                _meetingStartTimeUtc = Math.Abs(diff.Value.TotalSeconds) < toleranceSeconds 
                    ? _meetingStartTimeUtcFromCountdown 
                    : start;
            }

            if (Log.IsEnabled(LogEventLevel.Debug))
            {
                Log.Logger.Debug("Meeting start time UTC = {MeetingStartTimeUtc}", _meetingStartTimeUtc);
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

    private static bool IsDeviationSignificant(TimeSpan deviation)
    {
        return Math.Abs(deviation.TotalSeconds) > SmallestDeviationSecs
               && Math.Abs(deviation.TotalMinutes) <= LargestDeviationMinutes;
    }

    private TimeSpan CalculateRemainingAdaptiveTimerValues(TalkScheduleItem talk)
    {
        var result = TimeSpan.Zero;

        var started = false;
        foreach (var item in _scheduleService.GetTalkScheduleItems())
        {
            if (item == talk)
            {
                started = true;
            }

            if (started && item.AllowAdaptive)
            {
                result = result.Add(item.PlannedDuration);
            }
        }

        return result;
    }

    private DateTime? CalculateWeekendStartTime(TalkScheduleItem talk)
    {
        DateTime? result = null;
        if (talk.Id == (int)TalkTypesAutoMode.PublicTalk)
        {
            result = GetNearest15MinsBefore(_dateTimeService.UtcNow());
        }

        return result;
    }

    private DateTime? CalculateMidWeekStartTime(TalkScheduleItem talk)
    {
        return talk.Id switch
        {
            (int) TalkTypesAutoMode.OpeningComments => GetNearest15MinsBefore(_dateTimeService.UtcNow()),
            (int) TalkTypesAutoMode.TreasuresTalk => GetNearest15MinsBefore(_dateTimeService.UtcNow()),
            _ => null
        };
    }

    private static DateTime GetNearest15MinsBefore(DateTime dateTimeBase)
    {
        var result = dateTimeBase.Date;
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

    private void OnCountdownWindowStatusChanged(object recipient, CountdownWindowStatusChangedMessage message)
    {
        if (!message.Showing)
        {
            _meetingStartTimeUtcFromCountdown = DateUtils.GetNearestMinute(_dateTimeService.UtcNow());
        }
        else
        {
            _meetingStartTimeUtcFromCountdown = null;
        }
    }
}