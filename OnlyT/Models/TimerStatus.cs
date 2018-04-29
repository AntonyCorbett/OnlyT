namespace OnlyT.Models
{
    using System;

    public class TimerStatus
    {
        public int? TalkId { get; set; }

        public int TargetSeconds { get; set; }

        public bool IsRunning { get; set; }

        public TimeSpan TimeElapsed { get; set; }
    }
}
