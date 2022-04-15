using OnlyT.CountdownTimer;

namespace OnlyT.Models
{
    public class CountdownElementsToShowItem
    {
        public CountdownElementsToShowItem(ElementsToShow elements, string name)
        {
            Elements = elements;
            Name = name;
        }

        public ElementsToShow Elements { get; }

        public string Name { get; }
    }
}
