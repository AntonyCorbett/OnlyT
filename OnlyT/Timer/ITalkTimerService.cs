using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.EventArgs;

namespace OnlyT.Timer
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
