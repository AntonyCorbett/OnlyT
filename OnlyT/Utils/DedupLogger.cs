using Serilog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

namespace OnlyT.Utils;

/// <summary>
/// Utility to prevent excessive logging of the same error.
/// </summary>
public class DedupLogger
{
    private readonly ConcurrentDictionary<string, long> _recentErrors = new();
    private const int ErrorSuppressWindowSeconds = 5;
    private const int MaxErrorEntries = 500;
    private const int CleanupBatchSize = 100;

    public void LogErrorDedup(Exception ex, string message)
    {
        var key = $"{ex.GetType().FullName}:{ex.Message}";
        var now = Stopwatch.GetTimestamp();

        if (_recentErrors.TryGetValue(key, out var lastLogged))
        {
            var seconds = (now - lastLogged) / (double)Stopwatch.Frequency;
            if (seconds < ErrorSuppressWindowSeconds)
            {
                // Suppress duplicate log
                return;
            }
        }

        _recentErrors[key] = now;

        Cleanup();

        Log.Logger.Error(ex, message);
    }

    private void Cleanup()
    {
        // Cleanup if too many entries
        if (_recentErrors.Count <= MaxErrorEntries)
        {
            return;
        }

        // there is a race condition here, but it's safe

        // Remove enough entries to bring the count down by CleanupBatchSize
        var count = _recentErrors.Count;
        var toRemove = Math.Min(count - MaxErrorEntries + CleanupBatchSize, count);
        var oldest = _recentErrors
            .OrderBy(kvp => kvp.Value)
            .Take(toRemove)
            .Select(kvp => kvp.Key)
            .ToArray();

#pragma warning disable U2U1203
        foreach (var oldKey in oldest)
#pragma warning restore U2U1203
        {
            _recentErrors.TryRemove(oldKey, out _);
        }
    }
}