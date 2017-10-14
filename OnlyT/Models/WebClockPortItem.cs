using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.Models
{
    public class WebClockPortItem
    {
        public int Port { get; set; }

        public string Name => Port.ToString();
    }
}
