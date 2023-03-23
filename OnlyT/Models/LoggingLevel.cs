using Serilog.Events;

namespace OnlyT.Models;

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