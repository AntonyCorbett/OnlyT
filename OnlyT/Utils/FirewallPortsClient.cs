using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.Utils
{
    internal static class FirewallPortsClient
    {
        public static int ReservePort(int port)
        {
            return LaunchFirewallPortsTool($"reserve {port}");
        }

        public static int OpenPort(int port)
        {
            return LaunchFirewallPortsTool($"open {port}");
        }

        public static int ClosePort(int port)
        {
            return LaunchFirewallPortsTool($"close {port}");
        }

        private static int LaunchFirewallPortsTool(string args)
        {
#if DEBUG
            var file = @"..\..\..\OnlyTFirewallPorts\bin\Debug\OnlyTFirewallPorts.exe";
#else
            var file = @"OnlyTFirewallPorts.exe";
#endif

            var psi = new ProcessStartInfo(file, args)
            {
                Verb = "runas",
                RedirectStandardOutput = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = true
            };

            var process = new Process { StartInfo = psi };
            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode;
        }

    }
}
