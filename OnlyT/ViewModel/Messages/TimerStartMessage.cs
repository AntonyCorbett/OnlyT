namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When the timer is started
    /// </summary>
    internal class TimerStartMessage
    {
        public int TargetSeconds { get; }
        public bool CountUp { get; }

        public TimerStartMessage(int targetSeconds, bool countUp)
        {
            TargetSeconds = targetSeconds;
            CountUp = countUp;
        }
    }
}
