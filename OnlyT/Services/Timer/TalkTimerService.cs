using System;
using System.Diagnostics;
using System.Windows.Threading;
using OnlyT.EventArgs;

namespace OnlyT.Services.Timer
{
   class TalkTimerService : ITalkTimerService
   {
      private readonly Stopwatch _stopWatch = new Stopwatch();
      private readonly DispatcherTimer _timer = new DispatcherTimer(DispatcherPriority.Render);
      private int _targetSecs = 600;
      private readonly TimeSpan _timerInterval = TimeSpan.FromMilliseconds(100);

      public event EventHandler<TimerChangedEventArgs> TimerChangedEvent;

      public TalkTimerService()
      {
         _timer.Interval = _timerInterval;
         _timer.Tick += TimerElapsedHandler;
      }

      private void TimerElapsedHandler(object sender, System.EventArgs e)
      {
         _timer.Stop();
         UpdateTimerValue();
         _timer.Start();
      }

      private void UpdateTimerValue()
      {
         CurrentSecondsElapsed = (int) _stopWatch.Elapsed.TotalSeconds;
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
