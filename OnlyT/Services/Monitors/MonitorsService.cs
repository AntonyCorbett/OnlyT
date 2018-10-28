namespace OnlyT.Services.Monitors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using Models;
    using Serilog;

    /// <summary>
    /// Service to get display device information
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
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

            var displayScreens = GetDisplayScreens(devices);

            foreach (var screen in Screen.AllScreens)
            {
                var displayScreen = displayScreens?.SingleOrDefault(x => x.Item1.Equals(screen));
                var deviceData = displayScreen?.Item2;

                var monitor = new MonitorItem
                {
                    Monitor = screen,
                    MonitorName = deviceData?.DeviceString ?? SanitizeScreenDeviceName(screen.DeviceName),
                    MonitorId = deviceData?.DeviceId ?? screen.DeviceName,
                    FriendlyName = screen.DeviceFriendlyName()
                };

                if (string.IsNullOrEmpty(monitor.FriendlyName))
                {
                    monitor.FriendlyName = monitor.MonitorName;
                }

                result.Add(monitor);
            }

            return result;
        }

        public MonitorItem GetMonitorItem(string monitorId)
        {
            return GetSystemMonitors().SingleOrDefault(x => x.MonitorId.Equals(monitorId));
        }

        private DisplayDeviceData GetDeviceMatchingScreen(DisplayDeviceData[] devices, Screen screen)
        {
            var deviceName = screen.DeviceName + "\\";
            return devices.SingleOrDefault(x => x.Name.StartsWith(deviceName));
        }

        private string SanitizeScreenDeviceName(string name)
        {
            return name.Replace(@"\\.\", string.Empty);
        }

        private List<(Screen, DisplayDeviceData)> GetDisplayScreens(DisplayDeviceData[] devices)
        {
            var result = new List<(Screen, DisplayDeviceData)>();

            foreach (var screen in Screen.AllScreens)
            {
                Log.Logger.Verbose($"Screen: {screen.DeviceName}");

                var deviceData = GetDeviceMatchingScreen(devices, screen);
                if (deviceData == null)
                {
                    return null;
                }

                Log.Logger.Verbose($"Matching device: {deviceData.DeviceString}, {deviceData.DeviceId}");
                result.Add((screen, deviceData));
            }

            return result;
        }
    }
}
