namespace OnlyT.Models
{
    public class WebClockPortItem
    {
        public int Port { get; set; }

        public string Name => Port.ToString();
    }
}
