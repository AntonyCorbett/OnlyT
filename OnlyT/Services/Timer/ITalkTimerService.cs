using System;
using OnlyT.EventArgs;
using OnlyT.Models;

namespace OnlyT.Services.Timer
{
    public interface ITalkTimerService
    {
        event EventHandler<TimerChangedEventArgs> TimerChangedEvent;
        void Start(int targetSecs);
        void Stop();
        int CurrentSecondsElapsed { get; set; }
        bool IsRunning { get; }
        ClockRequestInfo GetClockRequestInfo();
    }
}
