using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Models;

namespace OnlyT.Services.Monitors
{
   public interface IMonitorsService
   {
      IEnumerable<MonitorItem> GetSystemMonitors();
   }
}
