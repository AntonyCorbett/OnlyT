namespace OnlyT.Utils
{
    using Serilog.Events;

    public static class EnumExtensions
    {
        public static string GetDescriptiveName(this LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Debug => Properties.Resources.LOG_LEVEL_DEBUG,
                LogEventLevel.Error => Properties.Resources.LOG_LEVEL_ERROR,
                LogEventLevel.Fatal => Properties.Resources.LOG_LEVEL_FATAL,
                LogEventLevel.Verbose => Properties.Resources.LOG_LEVEL_VERBOSE,
                LogEventLevel.Warning => Properties.Resources.LOG_LEVEL_WARNING,
                // ReSharper disable once RedundantCaseLabel
                LogEventLevel.Information => Properties.Resources.LOG_LEVEL_INFORMATION,
                _ => Properties.Resources.LOG_LEVEL_INFORMATION
            };
        }
    }
}
