using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.Options;

namespace OnlyT.Services.CountdownTimer
{
    public interface ICountdownTimerTriggerService
    {
        bool IsInCountdownPeriod(out int secondsOffset);
    }
}
