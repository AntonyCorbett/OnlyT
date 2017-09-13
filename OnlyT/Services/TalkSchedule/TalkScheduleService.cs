using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Models;
using OnlyT.Services.Options;

namespace OnlyT.Services.TalkSchedule
{
   /// <summary>
   /// Service to handle the delivery of a talk schedule based on current "Operating mode"
   /// </summary>
   public class TalkScheduleService : ITalkScheduleService
   {
      private readonly IOptionsService _optionsService;

      // the "talk_schedule.xml" file may exist in MyDocs\OnlyT..
      private Lazy<IEnumerable<TalkScheduleItem>> _fileBasedSchedule;
      private Lazy<IEnumerable<TalkScheduleItem>> _autoSchedule;

      public TalkScheduleService(IOptionsService optionsService)
      {
         _optionsService = optionsService;
         Reset();
      }

      public void Reset()
      {
         _fileBasedSchedule = new Lazy<IEnumerable<TalkScheduleItem>>(TalkScheduleFileBased.Read);
         _autoSchedule = new Lazy<IEnumerable<TalkScheduleItem>>(() => TalkScheduleAuto.Read(_optionsService));
      }

      public IEnumerable<TalkScheduleItem> GetTalkScheduleItems()
      {
         switch (_optionsService.Options.OperatingMode)
         {
            case OperatingMode.ScheduleFile:
               return _fileBasedSchedule.Value;
            case OperatingMode.Automatic:
               return _autoSchedule.Value;

            default:
            // ReSharper disable once RedundantCaseLabel
            case OperatingMode.Manual:
               return null;
         }
      }

      public TalkScheduleItem GetTalkScheduleItem(int id)
      {
         return GetTalkScheduleItems()?.SingleOrDefault(n => n.Id == id);
      }

      public int GetNext(int currentTalkId)
      {
         var talks = GetTalkScheduleItems().ToArray();
         for (int n = 0; n < talks.Length; ++n)
         {
            if (talks[n].Id.Equals(currentTalkId) && n != talks.Length - 1)
            {
               return talks[n + 1].Id;
            }
         }

         return 0;
      }
   }
}
