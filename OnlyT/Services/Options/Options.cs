using System.Globalization;

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
         MidWeekAdaptiveMode = AdaptiveMode.None;
         WeekendAdaptiveMode = AdaptiveMode.None;
         AnalogueClockWidthPercent = 50;

         var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;
         ClockHourFormat = dateFormat.Contains("H") ? ClockHourFormat.Format24LeadingZero : ClockHourFormat.Format12;
      }

      public string TimerMonitorId { get; set; }
      public string AppWindowPlacement { get; set; }
      public OperatingMode OperatingMode { get; set; }
      public MidWeekOrWeekend MidWeekOrWeekend { get; set; }
      public bool IsCircuitVisit { get; set; }
      public bool AlwaysOnTop { get; set; }
      public bool IsBellEnabled { get; set; }
      public int BellVolumePercent { get; set;}
      public ClockHourFormat ClockHourFormat { get; set; }
      public AdaptiveMode MidWeekAdaptiveMode { get; set; }
      public AdaptiveMode WeekendAdaptiveMode { get; set; }
      public int AnalogueClockWidthPercent { get; set; }


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

         if (AnalogueClockWidthPercent < 0 || AnalogueClockWidthPercent > 100)
         {
            AnalogueClockWidthPercent = 50;
         }
      }
   }
}
