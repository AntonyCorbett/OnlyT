using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OnlyT.Models;
using OnlyT.Services.Options;
using Serilog;

namespace OnlyT.Services.TalkSchedule
{
   internal static class TalkScheduleAuto
   {
      public static IEnumerable<TalkScheduleItem> Read(IOptionsService optionsService)
      {
         bool isCircuitVisit = optionsService.Options.IsCircuitVisit;

         return optionsService.Options.MidWeekOrWeekend == MidWeekOrWeekend.Weekend 
            ? GetWeekendMeetingSchedule(isCircuitVisit) 
            : GetMidweekMeetingSchedule(isCircuitVisit);
      }

      private static List<TalkScheduleItem> GetMidweekMeetingSchedule(bool isCircuitVisit)
      {
         var result = new List<TalkScheduleItem>();

         // Treasures...
         result.Add(new TalkScheduleItem(TalkTypesAutoMode.OpeningComments)
         {
            Name = "Opening Comments",
            Duration = TimeSpan.FromMinutes(3)
         });

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.TreasuresTalk)
         {
            Name = "Treasures Talk",
            Duration = TimeSpan.FromMinutes(10)
         });

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.DiggingTalk)
         {
            Name = "Digging for Spiritual Gems",
            Duration = TimeSpan.FromMinutes(8)
         });

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.Reading)
         {
            Name = "Bible Reading",
            Duration = TimeSpan.FromMinutes(4)
         });

         // Ministry...
         result.Add(new TalkScheduleItem(TalkTypesAutoMode.PresentationVideos)
         {
            Name = "This Month's Presentations",
            Duration = TimeSpan.FromMinutes(15)
         });

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.InitialCall)
         {
            Name = "Initial Call",
            Duration = TimeSpan.FromMinutes(2)
         });

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.ReturnVisit)
         {
            Name = "Return Visit",
            Duration = TimeSpan.FromMinutes(4)
         });

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.BibleStudy)
         {
            Name = "Bible Study / Talk",
            Duration = TimeSpan.FromMinutes(6)
         });

         // Living...
         result.Add(new TalkScheduleItem(TalkTypesAutoMode.LivingPart1)
         {
            Name = "Living - Part 1",
            Duration = TimeSpan.FromMinutes(8)
         });

         result.Add(new TalkScheduleItem(TalkTypesAutoMode.LivingPart2)
         {
            Name = "Living - Part 2",
            Duration = TimeSpan.FromMinutes(7)
         });

         if (isCircuitVisit)
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.ConcludingComments)
            {
               Name = "Concluding Comments",
               Duration = TimeSpan.FromMinutes(3)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.CircuitServiceTalk)
            {
               Name = "Service Talk",
               Duration = TimeSpan.FromMinutes(30)
            });
         }
         else
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.CongBibleStudy)
            {
               Name = "Congregation Bible Study",
               Duration = TimeSpan.FromMinutes(30)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.ConcludingComments)
            {
               Name = "Concluding Comments",
               Duration = TimeSpan.FromMinutes(3)
            });
         }

         return result;
      }

      private static List<TalkScheduleItem> GetWeekendMeetingSchedule(bool isCircuitVisit)
      {
         var result = new List<TalkScheduleItem>();
         
         if (isCircuitVisit)
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.PublicTalk)
            {
               Name = "Public Talk",
               Duration = TimeSpan.FromMinutes(30)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.Watchtower)
            {
               Name = "Watchtower Study",
               Duration = TimeSpan.FromMinutes(30)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.CircuitServiceTalk)
            {
               Name = "Concluding Talk",
               Duration = TimeSpan.FromMinutes(30)
            });
         }
         else
         {
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.PublicTalk)
            {
               Name = "Public Talk",
               Duration = TimeSpan.FromMinutes(30)
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.Watchtower)
            {
               Name = "Watchtower Study",
               Duration = TimeSpan.FromMinutes(60)
            });
         }

         return result;
      }
   }
}
