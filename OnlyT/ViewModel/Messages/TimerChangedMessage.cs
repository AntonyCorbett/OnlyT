namespace OnlyT.ViewModel.Messages;

/// <summary>
/// When the timer changes
/// </summary>
internal sealed class TimerChangedMessage
{
    public TimerChangedMessage(int remainingSecs, int elapsedSecs, bool timerIsRunning, int closingSecs, bool countUp)
    {
        RemainingSecs = remainingSecs;
        ElapsedSecs = elapsedSecs;
        TimerIsRunning = timerIsRunning;
        ClosingSecs = closingSecs;
        CountUp = countUp;
    }

    public int RemainingSecs { get; }

    public int ElapsedSecs { get; }

    public bool TimerIsRunning { get; }

    public int ClosingSecs { get; }

    public bool CountUp { get; }
}
