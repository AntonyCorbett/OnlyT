using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Models;

namespace OnlyT.Services.TalkSchedule
{
   public class TalkScheduleService : ITalkScheduleService
   {
      private static TalkScheduleSource _scheduleSource = TalkScheduleSource.Unknown;
      private readonly Lazy<IEnumerable<TalkScheduleItem>> _cachedSchedule =
         new Lazy<IEnumerable<TalkScheduleItem>>(ValueFactory);

      private static IEnumerable<TalkScheduleItem> ValueFactory()
      {
         var items = TalkScheduleFile.Read();
         if (items != null)
         {
            // schedule has been provided by an xml file...
            _scheduleSource = TalkScheduleSource.FromFile;
         }
         else
         {
            items = new List<TalkScheduleItem>
            {
               new TalkScheduleItem {Id = 100, Name = "Sample 3-Minute Item", Duration = TimeSpan.FromMinutes(3) },
               new TalkScheduleItem {Id = 101, Name = "Sample 10-Minute Item", Duration = TimeSpan.FromMinutes(10) },
               new TalkScheduleItem {Id = 102, Name = "Sample 20-Minute Item", Duration = TimeSpan.FromMinutes(20) },
            };
            _scheduleSource = TalkScheduleSource.HardCoded;
         }

         return items;
      }


      public IEnumerable<TalkScheduleItem> GetTalkScheduleItems()
      {
         return _cachedSchedule.Value;
      }

      public TalkScheduleItem GetTalkScheduleItem(int id)
      {
         return _cachedSchedule.Value.SingleOrDefault(n => n.Id == id);
      }
   }
}
