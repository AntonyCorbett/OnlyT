using System.Collections.Generic;
using OnlyT.Models;

namespace OnlyT.Services.Monitors
{
    public interface IMonitorsService
    {
        IEnumerable<MonitorItem> GetSystemMonitors();
        MonitorItem GetMonitorItem(string monitorId);
    }
}
