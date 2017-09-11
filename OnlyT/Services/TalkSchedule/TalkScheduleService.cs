using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Models;
using OnlyT.Services.Options;

namespace OnlyT.Services.TalkSchedule
{
   public class TalkScheduleService : ITalkScheduleService
   {
      private readonly IOptionsService _optionsService;

      // the "talk_schedule.xml" file may exist in MyDocs\OnlyT..
      private readonly Lazy<IEnumerable<TalkScheduleItem>> _cachedFileSchedule =
         new Lazy<IEnumerable<TalkScheduleItem>>(TalkScheduleFile.Read);

      public TalkScheduleService(IOptionsService optionsService)
      {
         _optionsService = optionsService;
      }
     
      public IEnumerable<TalkScheduleItem> GetTalkScheduleItems()
      {
         return _cachedFileSchedule.Value;
      }

      public TalkScheduleItem GetTalkScheduleItem(int id)
      {
         return _cachedFileSchedule.Value.SingleOrDefault(n => n.Id == id);
      }
   }
}
