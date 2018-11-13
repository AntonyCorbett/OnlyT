namespace OnlyT.Utils
{
    using Serilog.Events;

    public static class EnumExtensions
    {
        public static string GetDescriptiveName(this LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Debug:
                    return Properties.Resources.LOG_LEVEL_DEBUG;

                case LogEventLevel.Error:
                    return Properties.Resources.LOG_LEVEL_ERROR;

                case LogEventLevel.Fatal:
                    return Properties.Resources.LOG_LEVEL_FATAL;

                case LogEventLevel.Verbose:
                    return Properties.Resources.LOG_LEVEL_VERBOSE;

                case LogEventLevel.Warning:
                    return Properties.Resources.LOG_LEVEL_WARNING;

                default:
                // ReSharper disable once RedundantCaseLabel
                case LogEventLevel.Information:
                    return Properties.Resources.LOG_LEVEL_INFORMATION;
            }
        }
    }
}
