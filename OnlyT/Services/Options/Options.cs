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
         OperatingMode = OperatingMode.Manual;
      }

      public string TimerMonitorId { get; set; }
      public string AppWindowPlacement { get; set; }
      public OperatingMode OperatingMode { get; set; }


      /// <summary>
      /// Validates the data, correcting automatically as required
      /// </summary>
      public void Sanitize()
      {
         
      }
   }
}
