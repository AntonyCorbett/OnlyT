namespace OnlyT.Services.Options
{
    using OnlyT.Models;

    public interface IOptionsService
    {
        Options Options { get; }
        
        bool IsTimerMonitorSpecified { get; }

        bool IsCountdownMonitorSpecified { get; }

        void Save();

        bool Use24HrClockFormat();

        AdaptiveMode GetAdaptiveMode();
    }
}
