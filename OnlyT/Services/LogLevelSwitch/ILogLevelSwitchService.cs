namespace OnlyT.Services.LogLevelSwitch
{
    using Serilog.Events;

    public interface ILogLevelSwitchService
    {
        void SetMinimumLevel(LogEventLevel level);
    }
}
