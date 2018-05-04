namespace OnlyT.Models
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    using Services.Options;

    /// <summary>
    /// Used for items in the settings page, "Full-screen mode"
    /// </summary>
    public class ClockHourFormatItem
    {
        public string Name { get; set; }

        public ClockHourFormat Format { get; set; }
    }
}
