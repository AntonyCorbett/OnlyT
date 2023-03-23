using Serilog.Events;

namespace OnlyT.Services.LogLevelSwitch;

public interface ILogLevelSwitchService
{
    void SetMinimumLevel(LogEventLevel level);
}