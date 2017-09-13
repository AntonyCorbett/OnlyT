using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OnlyT.MeetingSongsFile;
using OnlyT.Models;
using OnlyT.Services.Options;
using OnlyT.Utils;
using Serilog;

namespace OnlyT.Services.TalkSchedule
{
   internal static class TalkScheduleAuto
   {
      public static IEnumerable<TalkScheduleItem> Read(IOptionsService optionsService)
      {
         bool isCircuitVisit = optionsService.Options.IsCircuitVisit;

         var finder = new MeetingSongsFinder();
         var songsAndTimers = finder.GetSongNumbersAndTimersForToday();

         return optionsService.Options.MidWeekOrWeekend == MidWeekOrWeekend.Weekend 
            ? GetWeekendMeetingSchedule(isCircuitVisit) 
            : GetMidweekMeetingSchedule(isCircuitVisit, songsAndTimers);
      }

      private static bool InFirstWeekOfMonth()
      {
         // ie according to workbook (where the first week starts on the first Monday of the month)...
         DateTime thisMonday = DateTime.Now.Prev(DayOfWeek.Monday);
         return thisMonday.Day <= 7;
      }

      private static List<TalkScheduleItem> GetTreasuresSchedule()
      {
         return new List<TalkScheduleItem>
         {
            new TalkScheduleItem(TalkTypesAutoMode.OpeningComments)
            {
               Name = Properties.Resources.TALK_OPENING_COMMENTS,
               Duration = TimeSpan.FromMinutes(3)
            },
            new TalkScheduleItem(TalkTypesAutoMode.TreasuresTalk)
            {
               Name = Properties.Resources.TALK_TREASURES,
               Duration = TimeSpan.FromMinutes(10)
            },
            new TalkScheduleItem(TalkTypesAutoMode.DiggingTalk)
            {
               Name = Properties.Resources.TALK_DIGGING,
               Duration = TimeSpan.FromMinutes(8)
            },
            new TalkScheduleItem(TalkTypesAutoMode.Reading)
            {
               Name = Properties.Resources.TALK_READING,
               Duration = TimeSpan.FromMinutes(4)
            }
         };
      }

      private static IEnumerable<TalkScheduleItem> GetMinistrySchedule()
      {
         var result = new List<TalkScheduleItem>();
         
         if (InFirstWeekOfMonth())
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.PresentationVideos)
            {
               Name = Properties.Resources.TALK_PRESENTATIONS,
               Duration = TimeSpan.FromMinutes(15)
            });
         }
         else
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.InitialCall)
            {
               Name = Properties.Resources.TALK_INITIAL_CALL,
               Duration = TimeSpan.FromMinutes(2)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.ReturnVisit)
            {
               Name = Properties.Resources.TALK_RV,
               Duration = TimeSpan.FromMinutes(4)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.BibleStudy)
            {
               Name = Properties.Resources.TALK_BIBLE_STUDY,
               Duration = TimeSpan.FromMinutes(6)
            });
         }

         return result;
      }

      private static IEnumerable<TalkScheduleItem> GetLivingSchedule(bool isCircuitVisit,
         MeetingSongsAndTimers songsAndTimers)
      {
         var result = new List<TalkScheduleItem>();
         
         int timerPart1 = 15;
         int timerPart2 = 0;

         if (songsAndTimers.TimersCount == 2)
         {
            timerPart1 = songsAndTimers.TimerValues[MeetingSongsAndTimers.LIVING_TIMER1_KEY];
            timerPart2 = songsAndTimers.TimerValues[MeetingSongsAndTimers.LIVING_TIMER2_KEY];
         }

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.LivingPart1)
         {
            Name = Properties.Resources.TALK_LIVING1,
            Duration = TimeSpan.FromMinutes(timerPart1)
         });

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.LivingPart2)
         {
            Name = Properties.Resources.TALK_LIVING2,
            Duration = TimeSpan.FromMinutes(timerPart2)
         });

         if (isCircuitVisit)
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.ConcludingComments)
            {
               Name = Properties.Resources.TALK_CONCLUDING_COMMENTS,
               Duration = TimeSpan.FromMinutes(3)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.CircuitServiceTalk)
            {
               Name = Properties.Resources.TALK_SERVICE,
               Duration = TimeSpan.FromMinutes(30)
            });
         }
         else
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.CongBibleStudy)
            {
               Name = Properties.Resources.TALK_CONG_STUDY,
               Duration = TimeSpan.FromMinutes(30)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.ConcludingComments)
            {
               Name = Properties.Resources.TALK_CONCLUDING_COMMENTS,
               Duration = TimeSpan.FromMinutes(3)
            });
         }

         return result;
      }


      private static List<TalkScheduleItem> GetMidweekMeetingSchedule(bool isCircuitVisit,
         MeetingSongsAndTimers songsAndTimers)
      {
         var result = new List<TalkScheduleItem>();

         // Treasures...
         result.AddRange(GetTreasuresSchedule());
         
         // Ministry...
         result.AddRange(GetMinistrySchedule());

         // Living...
         result.AddRange(GetLivingSchedule(isCircuitVisit, songsAndTimers));

         return result;
      }


      private static List<TalkScheduleItem> GetWeekendMeetingSchedule(bool isCircuitVisit)
      {
         var result = new List<TalkScheduleItem>();

         if (isCircuitVisit)
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.PublicTalk)
            {
               Name = Properties.Resources.TALK_PUBLIC,
               Duration = TimeSpan.FromMinutes(30)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.Watchtower)
            {
               Name = Properties.Resources.TALK_WT,
               Duration = TimeSpan.FromMinutes(30)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.CircuitServiceTalk)
            {
               Name = Properties.Resources.TALK_CONCLUDING,
               Duration = TimeSpan.FromMinutes(30)
            });
         }
         else
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.PublicTalk)
            {
               Name = Properties.Resources.TALK_PUBLIC,
               Duration = TimeSpan.FromMinutes(30)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.Watchtower)
            {
               Name = Properties.Resources.TALK_WT,
               Duration = TimeSpan.FromMinutes(60)
            });
         }

         return result;
      }
   }
}
