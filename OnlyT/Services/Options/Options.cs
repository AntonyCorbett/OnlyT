using System.Globalization;
using System.Windows;

namespace OnlyT.Services.Options
{
    /// <summary>
    /// All program options. The full structure is written to disk in JSON format on change
    /// of data, and read from disk during app startup
    /// </summary>
    public class Options
    {
        public static int DefaultPort = 8096;
        public static int MaxPossiblePorts = 0x80;

        public Options()
        {
            OperatingMode = OperatingMode.Automatic;
            AlwaysOnTop = true;
            IsBellEnabled = true;
            BellVolumePercent = 70;
            MidWeekAdaptiveMode = AdaptiveMode.None;
            WeekendAdaptiveMode = AdaptiveMode.None;
            AnalogueClockWidthPercent = 50;
            FullScreenClockMode = FullScreenClockMode.AnalogueAndDigital;
            ShowDurationSector = true;
            HttpServerPort = DefaultPort;

            var dateFormat = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern;

            bool clock24 = dateFormat.Contains("H");
            bool leadingZero = dateFormat.Contains("HH") || dateFormat.Contains("hh");
            bool ampm = dateFormat.Contains("tt");

            if (clock24)
            {
                ClockHourFormat = leadingZero ? ClockHourFormat.Format24LeadingZero : ClockHourFormat.Format24;
            }
            else
            {
                if (leadingZero)
                {
                    ClockHourFormat = ampm 
                        ? ClockHourFormat.Format12LeadingZeroAMPM : ClockHourFormat.Format12LeadingZero;
                }
                else
                {
                    ClockHourFormat = ampm
                        ? ClockHourFormat.Format12AMPM : ClockHourFormat.Format12;
                }
            }
        }

        public string TimerMonitorId { get; set; }
        public string AppWindowPlacement { get; set; }
        public Size SettingsPageSize { get; set; }
        public OperatingMode OperatingMode { get; set; }
        public MidWeekOrWeekend MidWeekOrWeekend { get; set; }
        public bool IsCircuitVisit { get; set; }
        public bool AlwaysOnTop { get; set; }
        public bool IsBellEnabled { get; set; }
        public int BellVolumePercent { get; set; }
        public ClockHourFormat ClockHourFormat { get; set; }
        public AdaptiveMode MidWeekAdaptiveMode { get; set; }
        public AdaptiveMode WeekendAdaptiveMode { get; set; }
        public int AnalogueClockWidthPercent { get; set; }
        public FullScreenClockMode FullScreenClockMode { get; set; }
        public bool ShowTimeOfDayUnderTimer { get; set; }
        public bool ShowDurationSector { get; set; }
        public bool CountUp { get; set; }
        public int HttpServerPort { get; set; }
        public bool IsWebClockEnabled { get; set; }
        public bool AllowCountUpToggle { get; set; }


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

            if (HttpServerPort < DefaultPort || HttpServerPort > DefaultPort + MaxPossiblePorts)
            {
                HttpServerPort = DefaultPort;
            }
        }
    }
}
