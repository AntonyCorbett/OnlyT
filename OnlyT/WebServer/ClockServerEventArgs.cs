using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.WebServer
{
    public class ClockServerEventArgs : System.EventArgs
    {
        public ClockServerMode Mode { get; set; }
        public int Mins { get; set; }
        public int Secs { get; set; }
        public int TargetSecs { get; set; }
    }
}
