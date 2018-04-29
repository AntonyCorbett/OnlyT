namespace OnlyT.Services.Monitors
{
    using System.Collections.Generic;
    using Models;

    public interface IMonitorsService
    {
        IEnumerable<MonitorItem> GetSystemMonitors();

        MonitorItem GetMonitorItem(string monitorId);
    }
}
