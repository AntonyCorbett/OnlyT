using OnlyT.Services.Options;

namespace OnlyT.Models;

/// <summary>
/// Used for items in the settings page, "Full-screen mode"
/// </summary>
public class ClockHourFormatItem
{
    public ClockHourFormatItem(string name, ClockHourFormat format)
    {
        Name = name;
        Format = format;
    }

    public string Name { get; }

    public ClockHourFormat Format { get; }
}