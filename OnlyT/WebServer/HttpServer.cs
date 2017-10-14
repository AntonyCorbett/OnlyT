using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;

namespace OnlyT.WebServer
{
    internal class HttpServer : IHttpServer, IDisposable
    {
        private HttpListener _listener;
        private int _port;
        private Task _task;
        private readonly bool _24HourClock;

        public event EventHandler<ClockServerEventArgs> ClockServerRequestHandler;

        public HttpServer()
        {
            _24HourClock = (new DateTime(1, 1, 1, 23, 1, 1)).ToShortTimeString().Contains("23");
        }

        public void Start(int port)
        {
            if (port > 0)
            {
                _listener = new HttpListener();
                _port = port;
                _task = Task.Factory.StartNew(StartListening, TaskCreationOptions.LongRunning);
            }
        }

        public void Stop()
        {
            _listener?.Stop();
        }

        private void StartListener()
        {
            _listener.Prefixes.Clear();
            _listener.Prefixes.Add($"http://*:{_port}/index/");
            _listener.Prefixes.Add($"http://*:{_port}/data/");

            _listener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            _listener.IgnoreWriteExceptions = true;
            _listener.Start();
        }

        private void StartListening()
        {
            try
            {
                StartListener();

                while (_listener.IsListening)
                {
                    try
                    {
                        IAsyncResult result = null;

                        if (_listener.IsListening)
                        {
                            result = _listener.BeginGetContext(ListenerCallback, _listener);
                        }

                        // Waiting for request to be processed
                        result?.AsyncWaitHandle.WaitOne();
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Could not start listening");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error during http listener initialisation");
            }
        }


        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                // Call EndGetContext to complete the asynchronous operation...
                HttpListenerContext context = _listener.EndGetContext(result);

                // Obtain a response object.
                using (HttpListenerResponse response = context.Response)
                {
                    try
                    {
                        // Construct a response. 
                        if (context.Request.Url.Segments.Length > 1)
                        {
                            // segments: "/" ...

                            string segment = context.Request.Url.Segments[1].TrimEnd('/').ToLower();
                            if (_listener.IsListening)
                            {
                                switch (segment)
                                {
                                    case "data":
                                        HandleRequestForTimerData(response);
                                        break;

                                    case "index":
                                        HandleRequestForTimerPage(response);
                                        break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "Clock server error");
                    }
                }
            }
        }

        private void HandleRequestForTimerPage(HttpListenerResponse response)
        {
            response.ContentType = "text/html";
            response.ContentEncoding = Encoding.UTF8;

            string responseString = Properties.Resources.ClockHtmlTemplate;
            if (!_24HourClock)
            {
                responseString = ReplaceTimeFormat(responseString);
            }

            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            using (System.IO.Stream output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
            }
        }

        private void HandleRequestForTimerData(HttpListenerResponse response)
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

        private string ReplaceTimeFormat(string responseString)
        {
            string origLineOfCode = "s = formatTimeOfDay(h, m, true);";
            string newLineOfCode = "s = formatTimeOfDay(h, m, false);";

            return responseString.Replace(origLineOfCode, newLineOfCode);
        }


        private string CreateXml()
        {
            StringBuilder sb = new StringBuilder();

            var args = new ClockServerEventArgs();
            OnClockServerRequest(args);

            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            sb.AppendLine("<root>");

            switch (args.Mode)
            {
                case ClockServerMode.Nameplate:
                    sb.AppendLine(" <clock mode=\"Nameplate\" mins=\"0\" secs=\"0\" targetSecs=\"0\" />");
                    break;

                case ClockServerMode.TimeOfDay:
                    // in this mode mins and secs hold the total offset time into the day
                    DateTime now = DateTime.Now;
                    sb.AppendLine(
                        $" <clock mode=\"TimeOfDay\" mins=\"{(now.Hour * 60) + now.Minute}\" secs=\"{now.Second}\" targetSecs=\"0\" />");
                    break;

                case ClockServerMode.Timer:
                    sb.AppendLine(
                        $" <clock mode=\"Timer\" mins=\"{args.Mins}\" secs=\"{args.Secs}\" targetSecs=\"{args.TargetSecs}\" />");
                    break;

                case ClockServerMode.TimerPause:
                    sb.AppendLine(
                        $" <clock mode=\"TimerPause\" mins=\"{args.Mins}\" secs=\"{args.Secs}\" targetSecs=\"{args.TargetSecs}\" />");
                    break;
            }

            sb.AppendLine("</root>");
            return sb.ToString();
        }

        private void OnClockServerRequest(ClockServerEventArgs e)
        {
            var handler = ClockServerRequestHandler;
            if (handler != null)
            {
                handler(this, e);
            }
            else
            {
                e.Mode = ClockServerMode.Nameplate;
            }
        }

        public void Dispose()
        {
            _listener.Close();
        }

    }
}
