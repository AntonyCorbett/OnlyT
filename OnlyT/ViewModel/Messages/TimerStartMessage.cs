namespace OnlyT.ViewModel.Messages;

/// <summary>
/// When the timer is started
/// </summary>
internal sealed class TimerStartMessage
{
    public TimerStartMessage(int targetSeconds, int elapsedSecs, bool countUp, int talkId)
    {
        TargetSeconds = targetSeconds;
        ElapsedSecs = elapsedSecs;
        CountUp = countUp;
        TalkId = talkId;
    }

    // when resuming after a pause, this is the time remaining (not the full talk duration)
    public int TargetSeconds { get; }

    // time already elapsed before this start (non-zero when resuming after a pause)
    public int ElapsedSecs { get; }

    public bool CountUp { get; }

    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public int TalkId { get; }
}