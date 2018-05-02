namespace OnlyT.EventArgs
{
    using Models;

    public class TimerStartStopEventArgs : System.EventArgs
    {
        public int TalkId { get; set; }

        public StartStopTimerCommands Command { get; set; }

        public bool Success { get; set; }

        public TimerStatus CurrentStatus { get; set; }
    }
}
