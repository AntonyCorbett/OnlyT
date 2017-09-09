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
         int mins = secsRemaining / _secsPerMinute;
         int secs = secsRemaining % _secsPerMinute;

         return $"{mins:D2}:{secs:D2}";
      }
   }
}
