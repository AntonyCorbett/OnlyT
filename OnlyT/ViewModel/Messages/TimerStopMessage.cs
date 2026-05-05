namespace OnlyT.ViewModel.Messages;

/// <summary>
/// When the timer is stopped
/// </summary>
internal sealed class TimerStopMessage
{
    public TimerStopMessage(int talkId, int elapsedSecs, bool persist, bool isPaused = false)
    {
        TalkId = talkId;
        ElapsedSecs = elapsedSecs;
        PersistFinalTimerValue = persist;
        IsPaused = isPaused;
    }

    public int TalkId { get; }

    public int ElapsedSecs { get; }

    public bool PersistFinalTimerValue { get; }

    public bool IsPaused { get; }
}