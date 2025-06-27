namespace OnlyT.WebServer.Controllers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text;
using EventArgs;
using NUglify;

internal sealed class WebPageController
{
    private static readonly Dictionary<WebPageTypes, Lazy<byte[]>> WebPageHtml = [];
    private readonly WebPageTypes _webPageType;

    public WebPageController(WebPageTypes webPageType)
    {
        _webPageType = webPageType;
        lock (WebPageHtml)
        {
            if (!WebPageHtml.ContainsKey(_webPageType))
            {
                string webPageTemplate = _webPageType switch
                {
                    WebPageTypes.Clock => Properties.Resources.ClockHtmlTemplate,
                    WebPageTypes.Timers => Properties.Resources.TimersHtmlTemplate,
                    _ => throw new NotSupportedException()
                };

                WebPageHtml.Add(_webPageType, new Lazy<byte[]>(() => GenerateWebPageHtml(webPageTemplate)));
            }
        }
    }

    public static void HandleRequestForTimerData(
        HttpListenerResponse response, 
        TimerInfoEventArgs timerInfo,
        DateTime now)
    {
        response.ContentType = "text/xml";
        response.ContentEncoding = Encoding.UTF8;

        var responseString = CreateXml(timerInfo, now);
        var buffer = Encoding.UTF8.GetBytes(responseString);

        response.ContentLength64 = buffer.Length;
        using var output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
    }

    public void HandleRequestForWebPage(
        HttpListenerResponse response)
    {
        response.ContentType = "text/html";
        response.ContentEncoding = Encoding.UTF8;
            
        response.ContentLength64 = WebPageHtml[_webPageType].Value.Length;
        using var output = response.OutputStream;
        output.Write(WebPageHtml[_webPageType].Value, 0, WebPageHtml[_webPageType].Value.Length);
    }

    private static byte[] GenerateWebPageHtml(string content)
    {
        var resourceKeys = new[] { "WEB_OFFLINE", "WEB_LINK_TIMERS", "WEB_LINK_CLOCK" };
        var resourceMan = new ResourceManager(typeof(Properties.Resources));

#pragma warning disable U2U1210 // Do not materialize an IEnumerable<T> unnecessarily
        resourceKeys.ToList().ForEach(k => content = content.Replace($"{{{k}}}", resourceMan.GetString(k)));
#pragma warning restore U2U1210 // Do not materialize an IEnumerable<T> unnecessarily

        return Encoding.UTF8.GetBytes(Uglify.Html(content).Code);
    }

    private static string CreateXml(TimerInfoEventArgs timerInfo, DateTime now)
    {
        var sb = new StringBuilder();
           
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<root>");

        switch (timerInfo.Mode)
        {
            case ClockServerMode.Nameplate:
                sb.AppendLine(" <clock mode=\"Nameplate\" mins=\"0\" secs=\"0\" ms=\"0\" targetSecs=\"0\" />");
                break;

            case ClockServerMode.TimeOfDay:
                // in this mode mins and secs hold the total offset time into the day
                var use24Hrs = timerInfo.Use24HrFormat ? 1 : 0;
                sb.AppendLine(
                    $" <clock mode=\"TimeOfDay\" mins=\"{(now.Hour * 60) + now.Minute}\" secs=\"{now.Second}\" ms=\"{now.Millisecond}\" targetSecs=\"0\" use24Hr=\"{use24Hrs}\"/>");
                break;

            case ClockServerMode.Timer:
                var countingUp = timerInfo.IsCountingUp ? 1 : 0;
                sb.AppendLine(
                    $" <clock mode=\"Timer\" mins=\"{timerInfo.Mins}\" secs=\"{timerInfo.Secs}\" ms=\"{timerInfo.Millisecs}\" targetSecs=\"{timerInfo.TargetSecs}\" countUp=\"{countingUp}\" closingSecs=\"{timerInfo.ClosingSecs}\"/>");
                break;

            default:
                throw new NotSupportedException();
        }

        sb.AppendLine("</root>");
        return sb.ToString();
    }
}