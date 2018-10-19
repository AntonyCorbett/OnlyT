namespace OnlyT.Services.Timer
{
    using System;
    using EventArgs;
    using Models;

    public interface ITalkTimerService
    {
        event EventHandler<TimerChangedEventArgs> TimerChangedEvent;

        event EventHandler<TimerStartStopEventArgs> TimerStartStopFromApiEvent;

        int CurrentSecondsElapsed { get; set; }

        bool IsRunning { get; }

        void Start(int targetSecs, int talkId);

        void Stop();

        ClockRequestInfo GetClockRequestInfo();

        TimerStatus GetStatus();

        void SetupTalk(int talkId, int targetSeconds);

        TimerStartStopEventArgs StartTalkTimerFromApi(int talkId);

        TimerStartStopEventArgs StopTalkTimerFromApi(int talkId);
    }
}
