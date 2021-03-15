namespace OnlyT.Models
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    using Services.Options;

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
}
