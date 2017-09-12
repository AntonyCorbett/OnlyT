using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Models;

namespace OnlyT.Services.TalkSchedule
{
   public interface ITalkScheduleService
   {
      IEnumerable<TalkScheduleItem> GetTalkScheduleItems();
      TalkScheduleItem GetTalkScheduleItem(int id);
      void Reset();
   }
}
