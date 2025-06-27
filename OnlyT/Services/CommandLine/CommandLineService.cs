﻿using System;
using System.Globalization;
using System.Net;
using Fclp;

namespace OnlyT.Services.CommandLine;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class CommandLineService : ICommandLineService
{
    // DefaultFeedUri refers to the json feed that provides information on meeting agenda items
    // for the automatic mode. If the default feed becomes unavailable or is no longer maintained
    // you can specify it on the command line, or you can modify the source here:

    private static readonly string DefaultFeedUri = "https://soundbox.blob.core.windows.net/meeting-feeds/feed.json";

    public CommandLineService()
    {
        var p = new FluentCommandLineParser();

        p.Setup<bool>("nogpu")
            .Callback(s => NoGpu = s).SetDefault(false);

        p.Setup<string>("id")
            .Callback(s => OptionsIdentifier = s).SetDefault(null!);

        p.Setup<bool>("nosettings")
            .Callback(s => NoSettings = s).SetDefault(false);

        p.Setup<bool>("nomutex")
            .Callback(s => IgnoreMutex = s).SetDefault(false);

        p.Setup<int>("monitor")
            .Callback(s => TimerMonitorIndex = s).SetDefault(0);

        p.Setup<int>("cmonitor")
            .Callback(s => CountdownMonitorIndex = s).SetDefault(0);

        p.Setup<bool>("automate")
            .Callback(s => Automate = s).SetDefault(false);

        p.Setup<string?>("datetime")
            .Callback(s =>
            {
                if (DateTime.TryParseExact(
                        s, 
                        "yyyy-MM-dd:HH:mm",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var result))
                {
                    DateTimeOnLaunch = result;
                }
            });

        p.Setup<bool>("covisit")
            .Callback(s => IsCircuitVisit = s).SetDefault(false);

        p.Setup<bool>("ndi")
            .Callback(s => IsTimerNdi = s).SetDefault(false);

        p.Setup<bool>("cndi")
            .Callback(s => IsCountdownNdi = s).SetDefault(false);

        p.Setup<string?>("ip")
            .Callback(SafeSetRemoteIpAddress).SetDefault(null);

        p.Setup<string?>("feed")
            .Callback(SafeSetFeedUri).SetDefault(DefaultFeedUri);

        p.Setup<string?>("docs")
            .Callback(SafeSetDocsFolder).SetDefault(null);

        p.Parse(Environment.GetCommandLineArgs());
    }

    private void SafeSetDocsFolder(string? folder)
    {
        OnlyTDocsFolder = string.IsNullOrWhiteSpace(folder) ? null : folder;
    }

    private void SafeSetRemoteIpAddress(string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            RemoteIpAddress = null;
        }
        else
        {
            RemoteIpAddress = IPAddress.TryParse(ipAddress, out var result) 
                ? result.ToString() 
                : null;
        }
    }

    private void SafeSetFeedUri(string? feedUri)
    {
        FeedUri = string.IsNullOrWhiteSpace(feedUri) ||
                  !Uri.IsWellFormedUriString(feedUri, UriKind.Absolute)
            ? DefaultFeedUri
            : feedUri;
    }

    public bool NoGpu { get; set; }

    public string? OptionsIdentifier { get; set; }

    public bool NoSettings { get; set; }

    public bool IgnoreMutex { get; set; }

    public int TimerMonitorIndex { get; set; }

    public int CountdownMonitorIndex { get; set; }

    public bool Automate { get; set; }

    public DateTime? DateTimeOnLaunch { get; set; }

    public bool IsCircuitVisit { get; set; }

    public bool IsTimerMonitorSpecified => TimerMonitorIndex > 0;

    public bool IsCountdownMonitorSpecified => CountdownMonitorIndex > 0;

    public bool IsTimerNdi { get; set; }

    public bool IsCountdownNdi { get; set; }

    public string? RemoteIpAddress { get; set; }

    public string FeedUri { get; set; } = DefaultFeedUri;

    public string? OnlyTDocsFolder { get; set; }
}