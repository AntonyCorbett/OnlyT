namespace OnlyT.Utils
{
    using System.Net;
    using System.Text;
    using System.Threading;

    internal static class WebUtils
    {
        const string UserAgentString = @"OnlyT (+https://soundboxsoftware.com)";

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
            using var wc = new WebClient { Encoding = Encoding.UTF8 };
            wc.Headers.Add("user-agent", UserAgentString);
            return wc.DownloadString(url);
        }
    }
}
