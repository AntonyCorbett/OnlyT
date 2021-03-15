namespace OnlyT.Utils
{
    using System.Net;
    using System.Text;
    using System.Threading;

    internal static class WebUtils
    {
        public static string? LoadWithUserAgent(string url)
        {
            const int attempts = 3;

            string? result = null;

            // multiple attempts to guard against transient exceptions.
            for (var n = 0; n < attempts && result == null; ++n)
            {
                try
                {
                    result = InternalLoadWithUserAgent(url);
                }
                catch (WebException)
                {
                    if (n == attempts - 1)
                    {
                        throw;
                    }

                    // ignore
                    Thread.Sleep(100);
                }
            }

            return result;
        }

        private static string InternalLoadWithUserAgent(string url)
        {
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("user-agent", GetUserAgentString());
                return wc.DownloadString(url);
            }
        }

        private static string GetUserAgentString()
        {
            return "OnlyT (+https://soundboxsoftware.com)";
        }
    }
}
