using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.ViewModel.Messages
{
    internal class StartCountDownMessage
    {
        public int DurationMinutes { get; }
        public int OffsetSeconds { get; }
    
        public StartCountDownMessage(int durationMins, int offsetSeconds)
        {
            DurationMinutes = durationMins;
            OffsetSeconds = offsetSeconds;
        }
    }
}
