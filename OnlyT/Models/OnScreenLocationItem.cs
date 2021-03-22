namespace OnlyT.Models
{
    using OnlyT.Services.Options;

    public class OnScreenLocationItem
    {
        public OnScreenLocationItem(string name, ScreenLocation location)
        {
            Name = name;
            Location = location;
        }

        public string Name { get; }

        public ScreenLocation Location { get; }
    }
}
