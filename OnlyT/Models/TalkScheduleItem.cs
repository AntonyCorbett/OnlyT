using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.TalkSchedule;

namespace OnlyT.Models
{
   public class TalkScheduleItem
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public TimeSpan Duration { get; set; }
      public bool Editable { get; set; }
      public bool Bell { get; set; }

      public TalkScheduleItem()
      {
         
      }

      public TalkScheduleItem(TalkTypesAutoMode tt)
      {
         Id = (int) tt;
      }

      public int GetDurationSeconds()
      {
         return (int)Duration.TotalSeconds;
      }
   }
}
