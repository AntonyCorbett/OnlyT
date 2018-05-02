namespace OnlyTFirewallPorts
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Environment.Exit(Execute(args));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-20);
            }
        }

        private static int Execute(string[] args)
        {
            int result = -10;
            
            if (args.Length == 2)
            {
                if (!int.TryParse(args[1], out var port))
                {
                    result = -1;
                }
                else if (args[0].Equals("reserveAndOpen"))
                {
                    Console.WriteLine($"Reserving and opening port {port}");
                    result = PortManager.ReserveAndOpenPort(port);
                }
                else if (args[0].Equals("reserve"))
                {
                    Console.WriteLine($"Reserving port {port}");
                    result = PortManager.ReservePort(port);
                }
                else if (args[0].Equals("open"))
                {
                    Console.WriteLine($"Opening port {port}");
                    result = PortManager.OpenPort(port);
                }
                else if (args[0].Equals("close"))
                {
                    Console.WriteLine($"Closing port {port}");
                    result = PortManager.ClosePort(port);
                }
                else
                {
                    result = -2;
                }
            }

            if (result != 0)
            {
                Console.Error.WriteLine($"Error {result}");
            }

            return result;
        }
    }
}
