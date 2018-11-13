namespace OnlyT.Services.LogLevelSwitch
{
    using Serilog.Core;
    using Serilog.Events;

    public class LogLevelSwitchService : ILogLevelSwitchService
    {
        public static readonly LoggingLevelSwitch LevelSwitch = new LoggingLevelSwitch
        {
            MinimumLevel = LogEventLevel.Information
        };

        public void SetMinimumLevel(LogEventLevel level)
        {
            LevelSwitch.MinimumLevel = level;
        }
    }
}
