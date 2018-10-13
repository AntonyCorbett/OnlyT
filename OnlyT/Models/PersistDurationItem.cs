namespace OnlyT.Models
{
    public class PersistDurationItem
    {
        public PersistDurationItem(int seconds)
        {
            Seconds = seconds;
        }

        public int Seconds { get; }

        public string Name => $"{Seconds} seconds";
    }
}
