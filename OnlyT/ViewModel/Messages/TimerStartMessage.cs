namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When the timer is started
    /// </summary>
    internal class TimerStartMessage
    {
        public int TargetSeconds { get; }

        public bool CountUp { get; }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int TalkId { get; }

        public TimerStartMessage(int targetSeconds, bool countUp, int talkId)
        {
            TargetSeconds = targetSeconds;
            CountUp = countUp;
            TalkId = talkId;
        }
    }
}
