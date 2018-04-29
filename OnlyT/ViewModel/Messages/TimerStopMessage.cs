namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When the timer is stopped
    /// </summary>
    internal class TimerStopMessage
    {
        public int TalkId { get; set; }

        public int ElapsedSecs { get; set; }


        public TimerStopMessage(int talkId, int elapsedSecs)
        {
            TalkId = talkId;
            ElapsedSecs = elapsedSecs;
        }
    }
}
