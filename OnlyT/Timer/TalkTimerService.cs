using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using OnlyT.EventArgs;


namespace OnlyT.Timer
{
   class TalkTimerService : ITalkTimerService
   {
      private readonly Stopwatch _stopWatch = new Stopwatch();
      private readonly System.Timers.Timer _timer = new System.Timers.Timer();
      private int _targetSecs = 600;
      private readonly int _timerIntervalMilliSecs = 250;

      public event EventHandler<TimerChangedEventArgs> TimerChangedEvent;

      public TalkTimerService()
      {
         _timer.AutoReset = false;
         _timer.Interval = _timerIntervalMilliSecs;
         _timer.Elapsed += TimerElapsedHandler;
      }

      private void TimerElapsedHandler(object sender, System.Timers.ElapsedEventArgs e)
      {
         UpdateTimerValue();
         _timer.Start();
      }

      private void UpdateTimerValue()
      {
         CurrentSecondsElapsed = (int)_stopWatch.Elapsed.TotalSeconds;
      }

      public void Start(int targetSecs)
      {
         _targetSecs = targetSecs;
         _stopWatch.Start();
         UpdateTimerValue();
         _timer.Start();
      }


      public void Stop()
      {
         _timer.Stop();
         _stopWatch.Reset();
         UpdateTimerValue();
      }

      private int _currentSecondsElapsed = 0;
      public int CurrentSecondsElapsed
      {
         get => _currentSecondsElapsed;
         set
         {
            if(_currentSecondsElapsed != value)
            {
               _currentSecondsElapsed = value;
               OnTimerChangedEvent(new TimerChangedEventArgs {TargetSecs = _targetSecs, ElapsedSecs = _currentSecondsElapsed });
            }
         }
      }

      public bool IsRunning => _stopWatch.IsRunning;

      protected virtual void OnTimerChangedEvent(TimerChangedEventArgs e)
      {
         TimerChangedEvent?.Invoke(this, e);
      }
   }
}
