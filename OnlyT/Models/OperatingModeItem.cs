using OnlyT.Services.Options;

namespace OnlyT.Models;

/// <summary>
/// Used for items in the Settings page, "Operating mode" combo
/// </summary>
public class OperatingModeItem
{
    public OperatingModeItem(string name, OperatingMode mode)
    {
        Name = name;
        Mode = mode;
    }

    public string Name { get; set; }

    public OperatingMode Mode { get; set; }
}