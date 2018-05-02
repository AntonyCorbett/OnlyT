namespace OnlyTFirewallPorts
{
    using System.Diagnostics;

    internal static class PortManager
    {
        private static string RuleNamePrefix = "OnlyTClockServer";

        public static int ReserveAndOpenPort(int port)
        {
            int rv1 = ReservePort(port);
            int rv2 = OpenPort(port);

            return rv1 == 0 || rv1 == 1 ? rv2 : rv1;
        }

        public static int ReservePort(int port)
        {
            string everyone = new System.Security.Principal.SecurityIdentifier(
                "S-1-1-0").Translate(typeof(System.Security.Principal.NTAccount)).ToString();

            string parameter = $"http add urlacl url=http://*:{port}/ user=\\{everyone}";
            return LaunchNetworkShell(parameter);
        }

        public static int OpenPort(int port)
        {
            string parameter = $"advfirewall firewall add rule name=\"{RuleNamePrefix}{port}\" dir=in action=allow protocol=TCP localport={port}";
            return LaunchNetworkShell(parameter);
        }

        public static int ClosePort(int port)
        {
            string parameter = $"advfirewall firewall delete rule name=\"{RuleNamePrefix}{port}\"";
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
