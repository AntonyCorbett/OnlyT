namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Net;
    using System.Text;
    using GalaSoft.MvvmLight.Messaging;
    using NUglify;
    using ViewModel.Messages;

    internal class ClockWebPageController
    {
        private static byte[] WebPageHtml;
        private static bool TwentyFourHourClockUsed;

        public void HandleRequestForTimerData(HttpListenerResponse response)
        {
            response.ContentType = "text/xml";
            response.ContentEncoding = Encoding.UTF8;

            string responseString = CreateXml();
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            using (System.IO.Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }

        public void HandleRequestForWebPage(HttpListenerResponse response, bool twentyFourHourClock)
        {
            EnsureWebPageHtmlPrepared(twentyFourHourClock);

            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            
            response.ContentLength64 = WebPageHtml.Length;
            using (System.IO.Stream output = response.OutputStream)
            {
                output.Write(WebPageHtml, 0, WebPageHtml.Length);
            }
        }

        private void EnsureWebPageHtmlPrepared(bool twentyFourHourClock)
        {
            if (WebPageHtml == null || TwentyFourHourClockUsed != twentyFourHourClock)
            {
                var content = Properties.Resources.ClockHtmlTemplate;
                if (!twentyFourHourClock)
                {
                    content = ReplaceTimeFormat(content);
                }
                
                WebPageHtml = Encoding.UTF8.GetBytes(Uglify.Html(content).Code);
                TwentyFourHourClockUsed = twentyFourHourClock;
            }
        }

        private string ReplaceTimeFormat(string responseString)
        {
            string origLineOfCode = "s = formatTimeOfDay(h, m, true);";
            string newLineOfCode = "s = formatTimeOfDay(h, m, false);";

            return responseString.Replace(origLineOfCode, newLineOfCode);
        }

        private void GetClockServerInfo(GetCurrentTimerInfoMessage message)
        {
            Messenger.Default.Send(message);
        }

        private string CreateXml()
        {
            var sb = new StringBuilder();

            var message = new GetCurrentTimerInfoMessage();
            GetClockServerInfo(message);

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<root>");

            switch (message.Mode)
            {
                case ClockServerMode.Nameplate:
                    sb.AppendLine(" <clock mode=\"Nameplate\" mins=\"0\" secs=\"0\" ms=\"0\" targetSecs=\"0\" />");
                    break;

                case ClockServerMode.TimeOfDay:
                    // in this mode mins and secs hold the total offset time into the day
                    DateTime now = DateTime.Now;
                    sb.AppendLine(
                        $" <clock mode=\"TimeOfDay\" mins=\"{(now.Hour * 60) + now.Minute}\" secs=\"{now.Second}\" ms=\"{now.Millisecond}\" targetSecs=\"0\" />");
                    break;

                case ClockServerMode.Timer:
                    sb.AppendLine(
                        $" <clock mode=\"Timer\" mins=\"{message.Mins}\" secs=\"{message.Secs}\" ms=\"{message.Millisecs}\" targetSecs=\"{message.TargetSecs}\" />");
                    break;

                case ClockServerMode.TimerPause:
                    sb.AppendLine(
                        $" <clock mode=\"TimerPause\" mins=\"{message.Mins}\" secs=\"{message.Secs}\" ms=\"{message.Millisecs}\" targetSecs=\"{message.TargetSecs}\" />");
                    break;
            }

            sb.AppendLine("</root>");
            return sb.ToString();
        }
    }
}
