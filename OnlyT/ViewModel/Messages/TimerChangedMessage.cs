namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When the timer changes
    /// </summary>
    internal class TimerChangedMessage
    {
        public int RemainingSecs { get; }

        public int ElapsedSecs { get; }

        public bool TimerIsRunning { get; }

        public bool CountUp { get; }

        public TimerChangedMessage(int remainingSecs, int elapsedSecs, bool timerIsRunning, bool countUp)
        {
            RemainingSecs = remainingSecs;
            ElapsedSecs = elapsedSecs;
            TimerIsRunning = timerIsRunning;
            CountUp = countUp;
        }
    }
}
