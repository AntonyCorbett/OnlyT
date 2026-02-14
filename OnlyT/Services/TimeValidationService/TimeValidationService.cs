using System;
using System.Net;
using System.Net.Sockets;
using Serilog;

namespace OnlyT.Services.TimeValidationService;

/// <summary>
/// Service to validate system time against an NTP server.
/// </summary>

internal static class TimeValidationService
{
    private const string NtpServer = "pool.ntp.org";
    private const int NtpPort = 123;
    private const int Timeout = 3000; // 3 seconds
    private const long MaxTimeDifferenceMs = 15000; // 15 secs

    /// <summary>
    /// Validates system time against NTP server. Returns corrected time if difference exceeds threshold,
    /// otherwise returns null to use system time.
    /// </summary>
    public static DateTime? GetValidatedTime()
    {
        try
        {
            var ntpTime = GetNtpTime();
            if (ntpTime == null)
            {
                return null;
            }

            // NB - don't use the DateTimeService below!
            var systemTime = DateTime.UtcNow;
            var difference = Math.Abs((ntpTime.Value - systemTime).TotalMilliseconds);

            if (difference > MaxTimeDifferenceMs)
            {
                Log.Logger.Warning("System time differs from NTP by {DifferenceMs}ms. Using NTP time instead.", difference);
                return ntpTime.Value.ToLocalTime();
            }

            return null; // System time is acceptable
        }
        catch (Exception ex)
        {
            Log.Logger.Warning(ex, "Failed to validate time against NTP server. Using system time.");
            return null;
        }
    }

    private static DateTime? GetNtpTime()
    {
        try
        {
            using var client = new UdpClient();
            client.Client.ReceiveTimeout = Timeout;

            var ntpData = new byte[48];
            ntpData[0] = 0x1b; // NTP version 3, client mode

            client.Send(ntpData, ntpData.Length, NtpServer, NtpPort);

            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            var receivedData = client.Receive(ref endpoint);
            if (receivedData.Length < 40)
            {
                return null;
            }

            const byte serverReplyTime = 40;
            var intPart = ((uint)receivedData[serverReplyTime] << 24) |
                          ((uint)receivedData[serverReplyTime + 1] << 16) |
                          ((uint)receivedData[serverReplyTime + 2] << 8) |
                          receivedData[serverReplyTime + 3];

            const long baseTicks = 2208988800; // Unix epoch difference
            var utcTicks = (long)intPart - baseTicks;
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(utcTicks);
        }
        catch (Exception ex)
        {
            Log.Logger.Debug(ex, "NTP request failed");
            return null;
        }
    }
}