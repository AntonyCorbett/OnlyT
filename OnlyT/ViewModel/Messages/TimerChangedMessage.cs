using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.ViewModel.Messages
{
   internal class TimerChangedMessage
   {
      public int RemainingSecs { get; }

      public TimerChangedMessage(int remainingSecs)
      {
         RemainingSecs = remainingSecs;
      }
   }
}
