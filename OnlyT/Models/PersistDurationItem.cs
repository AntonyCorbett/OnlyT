namespace OnlyT.Models
{
    public class PersistDurationItem
    {
        public int Seconds { get; }

        public string Name => $"{Seconds} seconds";

        public PersistDurationItem(int seconds)
        {
            Seconds = seconds;
        }
    }
}
