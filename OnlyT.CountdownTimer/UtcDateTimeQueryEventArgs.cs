namespace OnlyT.CountdownTimer
{
    using System;

    public class UtcDateTimeQueryEventArgs : EventArgs
    {
        public DateTime UtcDateTime { get; set; }
    }
}
