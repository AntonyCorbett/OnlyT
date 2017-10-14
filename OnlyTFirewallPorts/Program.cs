using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyTFirewallPorts
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Environment.Exit(Execute(args));
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(-20);
            }
        }

        static int Execute(string[] args)
        {
            int result = -10;
            
            if (args.Length == 2)
            {
                if (!Int32.TryParse(args[1], out var port))
                {
                    result = -1;
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
