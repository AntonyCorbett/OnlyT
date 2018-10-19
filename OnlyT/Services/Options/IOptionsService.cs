namespace OnlyT.Services.Options
{
    public interface IOptionsService
    {
        Options Options { get; }
        
        bool IsTimerMonitorSpecified { get; }

        void Save();
    }
}
