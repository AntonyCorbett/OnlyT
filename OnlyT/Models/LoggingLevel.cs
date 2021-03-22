namespace OnlyT.Models
{
    using Serilog.Events;

    public class LoggingLevel
    {
        public LoggingLevel(string name, LogEventLevel level)
        {
            Name = name;
            Level = level;
        }

        public string Name { get; }

        public LogEventLevel Level { get; }
    }
}
