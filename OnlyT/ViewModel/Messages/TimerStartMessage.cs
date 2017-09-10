using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.ViewModel.Messages
{
   internal class TimerStartMessage
   {
      public int TargetSeconds { get; }

      public TimerStartMessage(int targetSeconds)
      {
         TargetSeconds = targetSeconds;
      }
   }
}
