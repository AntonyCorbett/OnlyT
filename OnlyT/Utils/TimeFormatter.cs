using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.Utils
{
   internal static class TimeFormatter
   {
      private static readonly int _secsPerMinute = 60;

      public static string FormatTimeRemaining(int secsRemaining)
      {
         int mins = Math.Abs(secsRemaining) / _secsPerMinute;
         int secs = Math.Abs(secsRemaining) % _secsPerMinute;

         return $"{mins:D2}:{secs:D2}";
      }

      //public static string FormatTimeOfDayHoursAndMins(DateTime dt)
      //{
      //   // todo: cater for 24 hr and 12 hr clocks...
      //   return $"{dt.Hour:D2}:{dt.Minute:D2}";
      //}

      //public static string FormatTimeOfDaySeconds(DateTime dt)
      //{
      //   return dt.Second.ToString("D2");
      //}
   }
}
