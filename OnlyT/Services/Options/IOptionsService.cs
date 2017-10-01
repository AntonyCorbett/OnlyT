namespace OnlyT.Services.Options
{
    public interface IOptionsService
    {
        Options Options { get; }
        void Save();

        bool IsTimerMonitorSpecified { get; }
    }
}
