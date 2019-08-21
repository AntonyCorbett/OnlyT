namespace OnlyT.Services.Options
{
    public interface IOptionsService
    {
        Options Options { get; }
        
        bool IsTimerMonitorSpecified { get; }

        bool IsCountdownMonitorSpecified { get; }

        bool IsTimerMonitorSetByCommandLine { get; }

        bool IsCountdownMonitorSetByCommandLine { get; }

        void Save();

        bool Use24HrClockFormat();

        AdaptiveMode GetAdaptiveMode();
    }
}
