using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace OnlyT.AutoUpdates
{
    internal static class VersionDetection
    {
        public static string LatestReleaseUrl => "https://github.com/AntonyCorbett/OnlyT/releases/latest";
        
        public static string GetLatestReleaseVersion()
        {
            string version = null;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = client.GetAsync(LatestReleaseUrl).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var latestVersionUri = response.RequestMessage.RequestUri;
                        if (latestVersionUri != null)
                        {
                            var segments = latestVersionUri.Segments;
                            if (segments.Any())
                            {
                                version = segments[segments.Length - 1];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Getting latest release version");
            }

            return version;
        }

        public static string GetCurrentVersion()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
        }
    }
}
