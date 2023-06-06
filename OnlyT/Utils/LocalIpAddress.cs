namespace OnlyT.Utils
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Windows.Documents;
    using OnlyT.EventTracking;
    using Serilog;

    internal static class LocalIpAddress
    {
        private static readonly Lazy<string> IpAddress = new(IpAddressFactory);

        public static string GetLocalIp4Address()
        {
            return IpAddress.Value;
        }

        private static string IpAddressFactory()
        {
            var result = string.Empty;
            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("10.0.2.4", 65530); // address doesn't need to exist!
                if (socket.LocalEndPoint is IPEndPoint endPoint)
                {
                    result = endPoint.Address.ToString();
                }
            }
            catch (Exception ex)
            {
                const string errMsg = "Could not get local IP Address";
                EventTracker.Error(ex, errMsg);

                Log.Logger.Warning(ex, errMsg);
            }
            
            return result;
        }
    }
}
