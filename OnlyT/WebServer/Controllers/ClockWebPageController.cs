namespace OnlyT.WebServer.Controllers
{
    using System;
    using System.Net;
    using System.Text;
    using EventArgs;
    using NUglify;

    internal class ClockWebPageController
    {
        private static readonly Lazy<byte[]> WebPageHtml = new Lazy<byte[]>(GenerateWebPageHtml);

        public void HandleRequestForTimerData(
            HttpListenerResponse response, 
            TimerInfoEventArgs timerInfo,
            DateTime now)
        {
            response.ContentType = "text/xml";
            response.ContentEncoding = Encoding.UTF8;

            string responseString = CreateXml(timerInfo, now);
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            using (System.IO.Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }

        public void HandleRequestForWebPage(
            HttpListenerResponse response)
        {
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;
            
            response.ContentLength64 = WebPageHtml.Value.Length;
            using (System.IO.Stream output = response.OutputStream)
            {
                output.Write(WebPageHtml.Value, 0, WebPageHtml.Value.Length);
            }
        }

        private static byte[] GenerateWebPageHtml()
        {
            var content = Properties.Resources.ClockHtmlTemplate;
            return Encoding.UTF8.GetBytes(Uglify.Html(content).Code);
        }

        private string CreateXml(TimerInfoEventArgs timerInfo, DateTime now)
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
                    int use24Hrs = timerInfo.Use24HrFormat ? 1 : 0;
                    sb.AppendLine(
                        $" <clock mode=\"TimeOfDay\" mins=\"{(now.Hour * 60) + now.Minute}\" secs=\"{now.Second}\" ms=\"{now.Millisecond}\" targetSecs=\"0\" use24Hr=\"{use24Hrs}\"/>");
                    break;

                case ClockServerMode.Timer:
                    int countingUp = timerInfo.IsCountingUp ? 1 : 0;
                    sb.AppendLine(
                        $" <clock mode=\"Timer\" mins=\"{timerInfo.Mins}\" secs=\"{timerInfo.Secs}\" ms=\"{timerInfo.Millisecs}\" targetSecs=\"{timerInfo.TargetSecs}\" countUp=\"{countingUp}\"/>");
                    break;
            }

            sb.AppendLine("</root>");
            return sb.ToString();
        }
    }
}
