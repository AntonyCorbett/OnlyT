using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.Utils
{
   /// <summary>
   /// Formats time values
   /// </summary>
   public static class TimeFormatter
   {
      private static readonly int _secsPerMinute = 60;

      /// <summary>
      /// Gets a time remaining string
      /// </summary>
      /// <param name="secsRemaining">Seconds remaining in talk (can be negative)</param>
      /// <returns>Formatted time (mins and secs)</returns>
      public static string FormatTimeRemaining(int secsRemaining)
      {
         int mins = Math.Abs(secsRemaining) / _secsPerMinute;
         int secs = Math.Abs(secsRemaining) % _secsPerMinute;

         return $"{mins:D2}:{secs:D2}";
      }
   }
}
