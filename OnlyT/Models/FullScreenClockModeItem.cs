namespace OnlyT.Models
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    using Services.Options;

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
}
