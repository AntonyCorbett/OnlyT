namespace OnlyT.Services.Timer
{
    using System;
    using EventArgs;
    using Models;

    public interface ITalkTimerService
    {
        event EventHandler<TimerChangedEventArgs> TimerChangedEvent;

        event EventHandler<TimerStartStopEventArgs> TimerStartStopFromApiEvent;

        event EventHandler<TimerDurationChangeEventArgs> TimerDurationChangeFromApiEvent;

        int CurrentSecondsElapsed { get; set; }

        bool IsRunning { get; }

        void Start(int targetSecs, int talkId, bool isCountingUp);

        void Stop();

        ClockRequestInfo GetClockRequestInfo();

        TimerStatus GetStatus();

        void SetupTalk(int talkId, int targetSeconds, int closingSecs);

        TimerStartStopEventArgs StartTalkTimerFromApi(int talkId);

        TimerStartStopEventArgs StopTalkTimerFromApi(int talkId);

        TimerDurationChangeEventArgs ChangeDurationFromApi(int talkId, int deltaSeconds);
    }
}
