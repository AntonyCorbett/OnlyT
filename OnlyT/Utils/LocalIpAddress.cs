using System;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace OnlyT.Utils
{
    internal class LocalIpAddress
    {
        private static readonly Lazy<string> IpAddress = new Lazy<string>(IpAddressFactory);

        private static string IpAddressFactory()
        {
            string result = string.Empty;
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("10.0.2.4", 65530); // address doesn't need to exist!
                    if (socket.LocalEndPoint is IPEndPoint endPoint)
                    {
                        result = endPoint.Address.ToString();
                    }
                }
            }
            // ReSharper disable once CatchAllClause
            catch (Exception ex)
            {
                Log.Logger.Warning(ex, "Could not get local IP Address");
            }
            
            return result;
        }

        public static string GetLocalIp4Address()
        {
            return IpAddress.Value;
        }
    }
}
