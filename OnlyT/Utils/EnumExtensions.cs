namespace OnlyT.Utils
{
    using Serilog.Events;

    public static class EnumExtensions
    {
        public static string GetDescriptiveName(this LogEventLevel level)
        {
            return level switch
            {
                // FYI, shown in order of increasing severity
                LogEventLevel.Verbose => Properties.Resources.LOG_LEVEL_VERBOSE,
                LogEventLevel.Debug => Properties.Resources.LOG_LEVEL_DEBUG,
                LogEventLevel.Information => Properties.Resources.LOG_LEVEL_INFORMATION,
                LogEventLevel.Warning => Properties.Resources.LOG_LEVEL_WARNING,
                LogEventLevel.Error => Properties.Resources.LOG_LEVEL_ERROR,
                LogEventLevel.Fatal => Properties.Resources.LOG_LEVEL_FATAL,
                _ => Properties.Resources.LOG_LEVEL_INFORMATION
            };
        }
    }
}
