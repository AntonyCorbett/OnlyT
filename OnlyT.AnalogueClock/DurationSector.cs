using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.AnalogueClock
{
   public class DurationSector
   {
      public double StartAngle { get; set; }
      public double CurrentAngle { get; set; }
      public double EndAngle { get; set; }

      public bool IsLargeArc => EndAngle - StartAngle >= 180.0;
   }
}
