using OnlyT.Services.Options;

namespace OnlyT.Models;

public class FullScreenClockModeItem
{
    public FullScreenClockModeItem(FullScreenClockMode mode, string name)
    {
        Mode = mode;
        Name = name;
    }

    public FullScreenClockMode Mode { get; }

    public string Name { get; }
}