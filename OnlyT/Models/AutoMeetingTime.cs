namespace OnlyT.Models
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    using Services.Options;

    /// <summary>
    /// Used for items in the Settings page, "Meeting" combo
    /// </summary>
    public class AutoMeetingTime
    {
        public MidWeekOrWeekend Id { get; set; }

        public string Name { get; set; }
    }
}
