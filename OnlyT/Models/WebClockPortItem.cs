namespace OnlyT.Models
{
    // ReSharper disable MemberCanBePrivate.Global
    public class WebClockPortItem
    {
        public int Port { get; set; }

        public string Name => Port.ToString();
    }
}
