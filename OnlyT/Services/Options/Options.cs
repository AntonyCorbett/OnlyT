namespace OnlyT.Services.Options
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using Models;
    using OnlyT.CountdownTimer;
    using Serilog.Events;

    /// <summary>
    /// All program options. The full structure is written to disk in JSON format on change
    /// of data, and read from disk during app startup
    /// </summary>
    public class Options
    {
        public const int DefaultPort = 8096;
        public const int MaxPossiblePorts = 0x80;
        public const int DefaultCountdownDurationMins = 5;

        public Options()
        {
            OperatingMode = OperatingMode.Automatic;
            CountdownScreenLocation = ScreenLocation.Centre;
            AlwaysOnTop = true;
            IsBellEnabled = true;
            AutoBell = true;
            BellVolumePercent = 70;
            MidWeekAdaptiveMode = AdaptiveMode.None;
            WeekendAdaptiveMode = AdaptiveMode.None;
            AnalogueClockWidthPercent = 50;
            CountdownZoomPercent = 100;
            CountdownDurationMins = DefaultCountdownDurationMins;
            FullScreenClockMode = FullScreenClockMode.AnalogueAndDigital;
            ShowDurationSector = true;
            HttpServerPort = DefaultPort;
            PersistDurationSecs = 90;
            IsApiThrottled = true;
            PersistStudentTime = true;
            MeetingStartTimes = new MeetingStartTimes.MeetingStartTimes();
            ShowDigitalSeconds = true;
            JwLibraryCompatibilityMode = true;
            CountdownFrame = true;
            TimerFrame = true;
            LogEventLevel = LogEventLevel.Information;
            ShowBackgroundOnClock = true;

            AdjustClockFormat();
        }

        public string TimerMonitorId { get; set; }

        public string CountdownMonitorId { get; set; }

        public bool CountdownFrame { get; set; }

        public ElementsToShow CountdownElementsToShow { get; set; }

        public bool ShowBackgroundOnTimer { get; set; }

        public bool ShowBackgroundOnClock { get; set; }

        public bool TimerFrame { get; set; }

        public bool ClockTimerFrame { get; set; }

        public string AppWindowPlacement { get; set; }

        public string TimerOutputWindowPlacement { get; set; }

        public string CountdownOutputWindowPlacement { get; set; }

        public Size SettingsPageSize { get; set; }

        public Size OperatorPageSize { get; set; }

        public OperatingMode OperatingMode { get; set; }

        public ScreenLocation CountdownScreenLocation { get; set; }

        public MidWeekOrWeekend MidWeekOrWeekend { get; set; }

        public bool IsCircuitVisit { get; set; }

        public bool ShowCircuitVisitToggle { get; set; }

        public bool PersistStudentTime { get; set; }

        public bool JwLibraryCompatibilityMode { get; set; }

        public bool AlwaysOnTop { get; set; }

        public bool IsBellEnabled { get; set; }

        public bool AutoBell { get; set; }

        public int BellVolumePercent { get; set; }

        public ClockHourFormat ClockHourFormat { get; set; }

        public bool ShowDigitalSeconds { get; set; }

        public AdaptiveMode MidWeekAdaptiveMode { get; set; }

        public AdaptiveMode WeekendAdaptiveMode { get; set; }

        public int AnalogueClockWidthPercent { get; set; }

        public int CountdownZoomPercent { get; set; }

        public int CountdownDurationMins { get; set; }

        public bool IsCountdownWindowTransparent { get; set; }

        public FullScreenClockMode FullScreenClockMode { get; set; }

        public bool ShowTimeOfDayUnderTimer { get; set; }

        public bool ShowDurationSector { get; set; }

        public bool CountUp { get; set; }

        public int HttpServerPort { get; set; }

        public int PersistDurationSecs { get; set; }

        public bool IsWebClockEnabled { get; set; }

        public bool AllowCountUpToggle { get; set; }

        public MeetingStartTimes.MeetingStartTimes MeetingStartTimes { get; set; }

        public bool IsApiEnabled { get; set; }

        public string ApiCode { get; set; }

        public bool IsApiThrottled { get; set; }

        public bool ShowMousePointerInTimerDisplay { get; set; }

        public string Culture { get; set; }

        public bool GenerateTimingReports { get; set; }

        public LogEventLevel LogEventLevel { get; set; }

        public bool ClockIsFlat { get; set; }

        public bool WeekendIncludesFriday { get; set; }

        public bool MainMonitorIsWindowed { get; set; }

        public bool CountdownMonitorIsWindowed { get; set; }

        public CountdownDurationItem[] GetCountdownDurationItems()
        {
            var result = new List<CountdownDurationItem>();

            for (int n = 0; n < 30; ++n)
            {
                result.Add(new CountdownDurationItem { DurationMins = n + 1 });
            }

            result.Add(new CountdownDurationItem { DurationMins = 45 });
            result.Add(new CountdownDurationItem { DurationMins = 60 });

            return result.ToArray();
        }

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

            if (CountdownZoomPercent < 10)
            {
                CountdownZoomPercent = 10;
            }

            if (CountdownZoomPercent > 100)
            {
                CountdownZoomPercent = 100;
            }

            SanitizeCountdownDurationMins();

            if (HttpServerPort < DefaultPort || HttpServerPort > DefaultPort + MaxPossiblePorts)
            {
                HttpServerPort = DefaultPort;
            }
            
            var persistDurations = GetPersistDurationItems();
            if (persistDurations.FirstOrDefault(x => x.Seconds == PersistDurationSecs) == null)
            {
                PersistDurationSecs = persistDurations[persistDurations.Length / 2].Seconds;
            }

            MeetingStartTimes.Sanitize();
        }

        public PersistDurationItem[] GetPersistDurationItems()
        {
            var result = new List<PersistDurationItem>();

            const int numItems = 11;
            const int secsIncrement = 15;

            int secs = secsIncrement;

            for (int n = 0; n < numItems; ++n)
            {
                result.Add(new PersistDurationItem(secs));
                secs += secsIncrement;
            }

            return result.ToArray();
        }

        private void AdjustClockFormat()
        {
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

        private void SanitizeCountdownDurationMins()
        {
            var validValues = GetCountdownDurationItems();

            if (validValues.All(x => x.DurationMins != CountdownDurationMins))
            {
                CountdownDurationMins = DefaultCountdownDurationMins;
            }
        }
    }
}
