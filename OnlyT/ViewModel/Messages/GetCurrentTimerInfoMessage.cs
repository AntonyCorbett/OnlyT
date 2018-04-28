using OnlyT.WebServer;

namespace OnlyT.ViewModel.Messages
{
    internal class GetCurrentTimerInfoMessage
    {
        public ClockServerMode Mode { get; set; }
        public int Mins { get; set; }
        public int Secs { get; set; }
        public int Millisecs { get; set; }
        public int TargetSecs { get; set; }
    }
}
