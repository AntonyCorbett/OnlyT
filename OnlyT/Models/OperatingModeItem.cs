namespace OnlyT.Models
{
    using Services.Options;

    /// <summary>
    /// Used for items in the Settings page, "Operating mode" combo
    /// </summary>
    public class OperatingModeItem
    {
        public string Name { get; set; }

        public OperatingMode Mode { get; set; }
    }
}
