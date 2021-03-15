namespace OnlyT.Models
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    using Services.Options;

    /// <summary>
    /// Used for items in the Settings page, "Meeting" combo
    /// </summary>
    public class AutoMeetingTime
    {
        public AutoMeetingTime(MidWeekOrWeekend id, string name)
        {
            Id = id;
            Name = name;
        }

        public MidWeekOrWeekend Id { get; }

        public string Name { get; }
    }
}
