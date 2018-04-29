namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When the timer is started
    /// </summary>
    internal class TimerStartMessage
    {
        public int TargetSeconds { get; }

        public bool CountUp { get; }

        public int TalkId { get; set; }

        public TimerStartMessage(int targetSeconds, bool countUp, int talkId)
        {
            TargetSeconds = targetSeconds;
            CountUp = countUp;
            TalkId = talkId;
        }
    }
}
