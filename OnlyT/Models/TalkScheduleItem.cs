using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.TalkSchedule;

namespace OnlyT.Models
{
   /// <summary>
   /// Represents a talk in the meeting schedule
   /// </summary>
   public class TalkScheduleItem
   {
      public int Id { get; set; }
      public string Name { get; set; }

      /// <summary>
      /// Original duration (before any user modification)
      /// </summary>
      public TimeSpan OriginalDuration { get; private set; }

      private TimeSpan _duration;
      public TimeSpan Duration
      {
         get => _duration;
         set
         {
            if (_duration != value)
            {
               _duration = value;
               if (OriginalDuration == default(TimeSpan))
               {
                  OriginalDuration = Duration;
               }
            }
         }
      }

      public bool Editable { get; set; }  // can the timer be modified manually?
      public bool Bell { get; set; } // should a bell be sounded at time-up?

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
