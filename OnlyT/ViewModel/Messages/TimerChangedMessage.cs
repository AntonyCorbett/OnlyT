namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When the timer changes
    /// </summary>
    internal class TimerChangedMessage
    {
        public int RemainingSecs { get; }
        public bool TimerIsRunning { get; }


        public TimerChangedMessage(int remainingSecs, bool timerIsRunning)
        {
            RemainingSecs = remainingSecs;
            TimerIsRunning = timerIsRunning;
        }
    }
}
