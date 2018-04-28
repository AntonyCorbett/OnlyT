using System;

namespace OnlyT.Models
{
    public class ClockRequestInfo
    {
        public int TargetSeconds { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        public bool IsRunning { get; set; }
    }
}
