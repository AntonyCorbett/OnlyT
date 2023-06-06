namespace OnlyT.ViewModel.Messages;

/// <summary>
/// When the timer is stopped
/// </summary>
internal sealed class TimerStopMessage
{
    public TimerStopMessage(int talkId, int elapsedSecs, bool persist)
    {
        TalkId = talkId;
        ElapsedSecs = elapsedSecs;
        PersistFinalTimerValue = persist;
    }

    public int TalkId { get; }

    public int ElapsedSecs { get; }

    public bool PersistFinalTimerValue { get; }
}