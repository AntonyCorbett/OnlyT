using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OnlyT.Models;
using Serilog;

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
            Log.Logger.Information("Getting system monitors");
            
            List<MonitorItem> result = new List<MonitorItem>();

            var devices = DisplayDevices.ReadDisplayDevices().ToArray();

            foreach (var screen in Screen.AllScreens)
            {
                Log.Logger.Information($"Screen: {screen.DeviceName}");
                
                DisplayDeviceData deviceData = GetDeviceMatchingScreen(devices, screen);
                if (deviceData != null)
                {
                    Log.Logger.Information($"Matching device: {deviceData.DeviceString}, {deviceData.DeviceId}");
                    
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
            var deviceName = screen.DeviceName + "\\";
            return devices.SingleOrDefault(x => x.Name.StartsWith(deviceName));
        }

        public MonitorItem GetMonitorItem(string monitorId)
        {
            return GetSystemMonitors().SingleOrDefault(x => x.MonitorId.Equals(monitorId));
        }
    }
}
