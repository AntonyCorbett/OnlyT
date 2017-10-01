using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OnlyT.Models;

namespace OnlyT.Services.Monitors
{
    /// <summary>
    /// Service to get display device information
    /// </summary>
    public sealed class MonitorsService : IMonitorsService
    {
        /// <summary>
        /// Gets a collection of system monitors
        /// </summary>
        /// <returns>Collection of MonitorItem</returns>
        public IEnumerable<MonitorItem> GetSystemMonitors()
        {
            List<MonitorItem> result = new List<MonitorItem>();

            var devices = DisplayDevices.ReadDisplayDevices().ToArray();

            foreach (var screen in Screen.AllScreens)
            {
                DisplayDeviceData deviceData = GetDeviceMatchingScreen(devices, screen);
                if (deviceData != null)
                {
                    result.Add(new MonitorItem
                    {
                        Monitor = screen,
                        MonitorName = deviceData.DeviceString,
                        MonitorId = deviceData.DeviceId
                    });
                }
            }

            return result;
        }

        private DisplayDeviceData GetDeviceMatchingScreen(DisplayDeviceData[] devices, Screen screen)
        {
            return devices.SingleOrDefault(x => x.Name.StartsWith(screen.DeviceName));
        }

        public MonitorItem GetMonitorItem(string monitorId)
        {
            return GetSystemMonitors().SingleOrDefault(x => x.MonitorId.Equals(monitorId));
        }
    }
}
