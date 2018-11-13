namespace OnlyT.Models
{
    using Serilog.Events;

    public class LoggingLevel
    {
        public string Name { get; set; }

        public LogEventLevel Level { get; set; }
    }
}
