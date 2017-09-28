using System;
using OnlyT.Services.Options;

namespace OnlyT.Services.Timer
{
   using System.Diagnostics;
   using System.Linq;

   using OnlyT.Models;
   using OnlyT.Services.TalkSchedule;

   /// <summary>
   /// Used in Automatic mode to automatically adapt talk timings according to
   /// the start time of the meeting and the start time of the talk. See also
   /// TalkScheduleAuto
   /// </summary>
   internal class AdaptiveTimerService : IAdaptiveTimerService
   {
      private static readonly int _largestDeviationMinutes = 15;

      private static readonly int _smallestDeviationSecs = 15;

      private DateTime? _meetingStartTimeUtc;

      private readonly IOptionsService _optionsService;

      private readonly ITalkScheduleService _scheduleService;

      public AdaptiveTimerService(IOptionsService optionsService, ITalkScheduleService scheduleService)
      {
         _optionsService = optionsService;
         _scheduleService = scheduleService;
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

      private void EnsureMeetingStartTimeIsSet(TalkScheduleItem talk)
      {
         if (_meetingStartTimeUtc == null)
         {
            SetMeetingStartUtc(talk);
         }
      }

      private bool DeviationWithinRange(TimeSpan deviation)
      {
         return Math.Abs(deviation.TotalSeconds) > _smallestDeviationSecs
                && Math.Abs(deviation.TotalMinutes) <= _largestDeviationMinutes;
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
               result = result.Add(item.Duration);
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
         Debug.Assert(_meetingStartTimeUtc != null);
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
            result = GetNearest15MinsBefore(DateTime.UtcNow.AddMinutes(-5));
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

      private DateTime GetNearest15MinsBefore(DateTime dtBase)
      {
         DateTime result = dtBase.Date;
         if (dtBase.Minute > 45)
         {
            result = result.AddHours(dtBase.Hour).AddMinutes(45);
         }
         else if (dtBase.Minute > 30)
         {
            result = result.AddHours(dtBase.Hour).AddMinutes(30);
         }
         else if (dtBase.Minute > 15)
         {
            result = result.AddHours(dtBase.Hour).AddMinutes(15);
         }
         else 
         {
            result = result.AddHours(dtBase.Hour);
         }
         return result;
      }

   }

}