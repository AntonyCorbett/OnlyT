using System;
using System.Globalization;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

internal static class AppCenterInit
{
    // Please omit this token (or use your own) if you are building a fork
    private static readonly string? TheToken = "02e4e166-6974-4675-9802-22131c351d3d";

    public static void Execute()
    {
        if (OperatingSystem.IsWindows())
        {
#pragma warning disable CA1416
            AppCenter.Start(TheToken, typeof(Analytics), typeof(Crashes));
            AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);
#pragma warning restore CA1416
        }
    }
}