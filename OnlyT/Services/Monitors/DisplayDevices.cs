using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Models;

namespace OnlyT.Services.Monitors
{
   /// <summary>
   /// Queries the system for information regarding display devices
   /// </summary>
   public static class DisplayDevices
   {
      /// <summary>
      /// Gets system display devices
      /// </summary>
      /// <returns>Collection of DisplayDeviceData</returns>
      public static IEnumerable<DisplayDeviceData> ReadDisplayDevices()
      {
         var result = new List<DisplayDeviceData>();

         NativeMethods.DISPLAY_DEVICE d = new NativeMethods.DISPLAY_DEVICE();
         d.cb = Marshal.SizeOf(d);

         for (uint id = 0; NativeMethods.EnumDisplayDevices(null, id, ref d, 0); id++)
         {
            if (d.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.AttachedToDesktop))
            {
               d.cb = Marshal.SizeOf(d);

               NativeMethods.EnumDisplayDevices(d.DeviceName, 0, ref d, 0);

               if (d.StateFlags.HasFlag(NativeMethods.DisplayDeviceStateFlags.AttachedToDesktop))
               {
                  result.Add(new DisplayDeviceData
                  {
                     Name = d.DeviceName,
                     DeviceId = d.DeviceID,
                     DeviceString = d.DeviceString,
                     DeviceKey = d.DeviceKey
                  });
               }
            }

            d.cb = Marshal.SizeOf(d);
         }

         return result;
      }

   }
}
