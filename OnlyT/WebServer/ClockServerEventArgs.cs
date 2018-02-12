namespace OnlyT.WebServer
{
    public class ClockServerEventArgs : System.EventArgs
    {
        public ClockServerMode Mode { get; set; }
        public int Mins { get; set; }
        public int Secs { get; set; }
        public int TargetSecs { get; set; }
    }
}
