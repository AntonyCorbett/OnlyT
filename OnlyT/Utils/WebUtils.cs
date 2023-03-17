namespace OnlyT.Utils
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    internal static class WebUtils
    {
        const string UserAgentString = "OnlyT (+https://soundboxsoftware.com)";

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
            using var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            using var request = new HttpRequestMessage { RequestUri = new(url), Method = HttpMethod.Get };
#pragma warning disable U2U1025 // Avoid instantiating HttpClient
            using var client = new HttpClient(handler);
#pragma warning restore U2U1025 // Avoid instantiating HttpClient
            client.DefaultRequestHeaders.Add("user-agent", UserAgentString);

            var response = client.Send(request);
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
