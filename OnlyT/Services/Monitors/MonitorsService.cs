using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Forms;
using OnlyT.Models;

namespace OnlyT.Services.Monitors
{
   public sealed class MonitorsService : IMonitorsService
   {
      public IEnumerable<MonitorItem> GetSystemMonitors()
      {
         List<MonitorItem> result = new List<MonitorItem>();

         var devices = DisplayDevices.ReadDisplayDevices().ToArray();

         foreach(var screen in Screen.AllScreens)
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
