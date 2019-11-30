namespace OnlyT.Services.Options
{
    using System;

    public interface IOptionsService
    {
        Options Options { get; }
        
        bool IsTimerMonitorSpecified { get; }

        bool IsCountdownMonitorSpecified { get; }

        bool IsTimerMonitorSetByCommandLine { get; }

        bool IsCountdownMonitorSetByCommandLine { get; }

        bool IsNowWeekend();

        bool IsWeekend(DateTime theDate);

        void Save();

        bool Use24HrClockFormat();

        AdaptiveMode GetAdaptiveMode();
    }
}
