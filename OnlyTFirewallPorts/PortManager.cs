namespace OnlyTFirewallPorts
{
    using System.Diagnostics;

    internal static class PortManager
    {
        private static readonly string RuleNamePrefix = "OnlyTClockServer";

        public static int ReserveAndOpenPort(int port)
        {
            var rv1 = ReservePort(port);
            var rv2 = OpenPort(port);
            
            return rv1 == 0 || rv1 == 1 ? rv2 : rv1;
        }

        public static int ReservePort(int port)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            var everyone = new System.Security.Principal.SecurityIdentifier(
                "S-1-1-0").Translate(typeof(System.Security.Principal.NTAccount)).ToString();
#pragma warning restore CA1416 // Validate platform compatibility

            var parameter = $"http add urlacl url=http://*:{port}/ user=\"{everyone}\"";
            return LaunchNetworkShell(parameter);
        }

        public static int OpenPort(int port)
        {
            ClosePort(port);

            var parameter = $"advfirewall firewall add rule name=\"{RuleNamePrefix}{port}\" dir=in action=allow protocol=TCP localport={port}";
            return LaunchNetworkShell(parameter);
        }

        public static int ClosePort(int port)
        {
            var parameter = $"advfirewall firewall delete rule name=\"{RuleNamePrefix}{port}\"";
            return LaunchNetworkShell(parameter);
        }

        private static int LaunchNetworkShell(string parameter)
        {
            var psi = new ProcessStartInfo("netsh", parameter)
            {
                Verb = "runas",
                RedirectStandardOutput = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            };

            var process = new Process { StartInfo = psi };
            process.Start();
            process.WaitForExit(5000);
            return process.ExitCode;
        }
    }
}
