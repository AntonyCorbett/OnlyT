namespace OnlyT.WebServer.Controllers;

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Resources;
using System.Text;
using System.Text.Json;
using EventArgs;
using NUglify;

internal sealed class WebPageController
{
    private static readonly ConcurrentDictionary<WebPageTypes, Lazy<byte[]>> WebPageHtml = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void HandleRequestForTimerData(
        HttpListenerResponse response,
        TimerInfoEventArgs timerInfo,
        DateTime now)
    {
        response.ContentType = "application/json";
        response.ContentEncoding = Encoding.UTF8;
        response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

        var responseString = CreateJson(timerInfo, now);
        var buffer = Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        using var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
    }

    public static void HandleRequestForWebPage(HttpListenerResponse response, WebPageTypes webPageType)
    {
        response.ContentType = "text/html";
        response.ContentEncoding = Encoding.UTF8;
        response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");

        var html = WebPageHtml.GetOrAdd(webPageType, t => new Lazy<byte[]>(() => GenerateWebPageHtml(t)));
        var bytes = html.Value;
        response.ContentLength64 = bytes.Length;
        using var output = response.OutputStream;
        output.Write(bytes, 0, bytes.Length);
    }

    private static byte[] GenerateWebPageHtml(WebPageTypes webPageType)
    {
        var template = webPageType switch
        {
            WebPageTypes.Clock => Properties.Resources.ClockHtmlTemplate,
            WebPageTypes.Timers => Properties.Resources.TimersHtmlTemplate,
            _ => throw new NotSupportedException()
        };

        return GenerateWebPageHtml(template);
    }

    private static byte[] GenerateWebPageHtml(string content)
    {
        content = content.Replace("{SHARED_JS}", Properties.Resources.SharedClockJs);
        content = content.Replace("{SNACKBAR}", Properties.Resources.SharedSnackbar);

        var resourceKeys = new[] { "WEB_OFFLINE", "WEB_LINK_TIMERS", "WEB_LINK_CLOCK" };
        var resourceMan = new ResourceManager(typeof(Properties.Resources));

        foreach (var k in resourceKeys)
            content = content.Replace($"{{{k}}}", resourceMan.GetString(k));

        return Encoding.UTF8.GetBytes(Uglify.Html(content).Code);
    }

    private static string CreateJson(TimerInfoEventArgs timerInfo, DateTime now)
    {
        var data = timerInfo.Mode switch
        {
            ClockServerMode.Nameplate => new ClockData { Mode = "Nameplate" },
            ClockServerMode.TimeOfDay => new ClockData
            {
                Mode = "TimeOfDay",
                Mins = (now.Hour * 60) + now.Minute,
                Secs = now.Second,
                Ms = now.Millisecond,
                Use24Hr = timerInfo.Use24HrFormat,
                ShowSecs = timerInfo.ShowTimeOfDaySeconds
            },
            ClockServerMode.Timer => new ClockData
            {
                Mode = "Timer",
                Mins = timerInfo.Mins,
                Secs = timerInfo.Secs,
                Ms = timerInfo.Millisecs,
                TargetSecs = timerInfo.TargetSecs,
                ClosingSecs = timerInfo.ClosingSecs,
                CountUp = timerInfo.IsCountingUp
            },
            ClockServerMode.Persist => new ClockData
            {
                Mode = "Persist",
                Mins = timerInfo.Mins,
                Secs = timerInfo.Secs,
                TargetSecs = timerInfo.TargetSecs,
                ClosingSecs = timerInfo.ClosingSecs,
                CountUp = timerInfo.IsCountingUp,
                ShowPersistBar = timerInfo.ShowPersistBar,
                PersistRemainingMs = timerInfo.PersistRemainingMs,
                PersistTotalMs = timerInfo.PersistTotalMs
            },
            _ => throw new NotSupportedException()
        };

        return JsonSerializer.Serialize(data, JsonOptions);
    }

    private sealed class ClockData
    {
        public string Mode { get; set; } = "Nameplate";
        public int Mins { get; set; }
        public int Secs { get; set; }
        public int Ms { get; set; }
        public int TargetSecs { get; set; }
        public int ClosingSecs { get; set; }
        public bool CountUp { get; set; }
        public bool Use24Hr { get; set; }
        public bool ShowSecs { get; set; }
        public bool ShowPersistBar { get; set; }
        public int PersistRemainingMs { get; set; }
        public int PersistTotalMs { get; set; }
    }
}
