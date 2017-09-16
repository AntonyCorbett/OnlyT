using System;
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

      private bool? _originalBell;

      public bool OriginalBell
      {
         get => _originalBell ?? false;
         private set
         {
            if (_originalBell == null)
            {
               _originalBell = value;
            }
         }
      }
      
      private bool _bell;
      // should a bell be sounded at time-up?
      public bool Bell
      {
         get => _bell;
         set
         {
            if (_bell != value)
            {
               _bell = value;
               OriginalBell = value;
            }
         }
      }

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
