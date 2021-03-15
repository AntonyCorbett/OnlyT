namespace OnlyT.Models
{
    using OnlyT.CountdownTimer;

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
