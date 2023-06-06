using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace OnlyT.EventTracking;

#pragma warning disable CA1416 // Validate platform compatibility

internal static class EventTracker
{
    public static void Track(EventName eventName, Dictionary<string, string>? properties = null)
    {
        Analytics.TrackEvent(eventName.ToString(), properties);
    }

    public static void Error(Exception ex, string? context = null)
    {
        if (string.IsNullOrEmpty(context))
        {
            Crashes.TrackError(ex);
        }
        else
        {
            var properties = new Dictionary<string, string> { { "context", context } };
            Crashes.TrackError(ex, properties);
        }
    }

    public static void TrackIncrement(int seconds)
    {
        Track(EventName.Increment, new Dictionary<string, string>
        {
            { "seconds", seconds.ToString(CultureInfo.InvariantCulture) },
        });
    }

    public static void TrackDecrement(int seconds)
    {
        Track(EventName.Decrement, new Dictionary<string, string>
        {
            { "seconds", seconds.ToString(CultureInfo.InvariantCulture) },
        });
    }

    public static void TrackCountdownMins(int minutes)
    {
        Track(EventName.CountDownMins, new Dictionary<string, string>
        {
            { "minutes", minutes.ToString(CultureInfo.InvariantCulture) },
        });
    }
}

#pragma warning restore CA1416 // Validate platform compatibility