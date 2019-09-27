namespace OnlyT.Models
{
    public class CountdownDurationItem
    {
        public string Name => DurationMins.ToString();

        public int DurationMins { get; set; }
    }
}
