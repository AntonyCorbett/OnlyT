using System;
using OnlyT.EventArgs;

namespace OnlyT.Services.Timer
{
   public interface ITalkTimerService
   {
      event EventHandler<TimerChangedEventArgs> TimerChangedEvent;
      void Start(int targetSecs);
      void Stop();
      int CurrentSecondsElapsed { get; set; }
      bool IsRunning { get; }
   }
}
