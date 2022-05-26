namespace OnlyT.Services.CommandLine
{
    public interface ICommandLineService
    {
        bool NoGpu { get; set; }

        string? OptionsIdentifier { get; set; }

        bool NoSettings { get; set; }

        bool IgnoreMutex { get; set; }

        bool Automate { get; set; }

        int TimerMonitorIndex { get; set; }

        int CountdownMonitorIndex { get; set; }
        
        bool IsCircuitVisit { get; set; }

        bool IsTimerMonitorSpecified { get; }

        bool IsCountdownMonitorSpecified { get; }
    }
}
