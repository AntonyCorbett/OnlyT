using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.Services.Options
{
   /// <summary>
   /// All program options. The full structure is written to disk in JSON format on change
   /// of data, and read from disk during app startup
   /// </summary>
   public class Options
   {
      public Options()
      {
         OperatingMode = OperatingMode.Automatic;
         AlwaysOnTop = true;
         IsBellEnabled = true;
         BellVolumePercent = 70;
      }

      public string TimerMonitorId { get; set; }
      public string AppWindowPlacement { get; set; }
      public OperatingMode OperatingMode { get; set; }
      public MidWeekOrWeekend MidWeekOrWeekend { get; set; }
      public bool IsCircuitVisit { get; set; }
      public bool AlwaysOnTop { get; set; }
      public bool IsBellEnabled { get; set; }
      public int BellVolumePercent { get; set;}


      /// <summary>
      /// Validates the data, correcting automatically as required
      /// </summary>
      public void Sanitize()
      {
         if (BellVolumePercent < 0)
         {
            BellVolumePercent = 0;
         }

         if (BellVolumePercent > 100)
         {
            BellVolumePercent = 100;
         }
      }
   }
}
