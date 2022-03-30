namespace OnlyT.Utils
{
    using System.Diagnostics;

    internal static class FirewallPortsClient
    {
        public static int ReserveAndOpenPort(int port)
        {
            return LaunchFirewallPortsTool($"reserveAndOpen {port}");
        }

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
            const string file = @"..\..\..\..\OnlyTFirewallPorts\bin\Debug\net5.0\OnlyTFirewallPorts.exe";
#else
            const string file = @"OnlyTFirewallPorts.exe";
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
