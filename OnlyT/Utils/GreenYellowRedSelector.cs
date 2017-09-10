using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace OnlyT.Utils
{
   internal static class GreenYellowRedSelector
   {
      private static readonly Brush _greenBrush = new SolidColorBrush(Colors.Chartreuse);
      private static readonly Brush _yellowBrush = new SolidColorBrush(Colors.Yellow);
      private static readonly Brush _redbrush = new SolidColorBrush(Colors.Red);

      public static Brush GetBrushForTimeRemaining(int secsRemaining)
      {
         if (secsRemaining <= 0)
         {
            return _redbrush;
         }

         if (secsRemaining <= 30)
         {
            return _yellowBrush;
         }

         return _greenBrush;
      }

      public static Brush GetGreenBrush()
      {
         return _greenBrush;
      }
   }
}
