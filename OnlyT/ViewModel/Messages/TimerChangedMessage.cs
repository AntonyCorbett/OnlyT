﻿namespace OnlyT.ViewModel.Messages;

/// <summary>
/// When the timer changes
/// </summary>
internal sealed class TimerChangedMessage
{
    public TimerChangedMessage(int remainingSecs, int elapsedSecs, bool timerIsRunning, bool countUp)
    {
        RemainingSecs = remainingSecs;
        ElapsedSecs = elapsedSecs;
        TimerIsRunning = timerIsRunning;
        CountUp = countUp;
    }

    public int RemainingSecs { get; }

    public int ElapsedSecs { get; }

    public bool TimerIsRunning { get; }

    public bool CountUp { get; }
}