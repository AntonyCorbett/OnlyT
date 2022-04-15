using OnlyT.Services.Options;

namespace OnlyT.Models
{
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
