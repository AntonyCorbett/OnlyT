using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OnlyT.Utils
{
   internal static class WebUtils
   {
      private static string GetUserAgentString()
      {
         return "OnlyT (+http://cv8.org.uk/soundbox)";
      }

      /// <summary>
      /// Downloads an xml file, specifying a user agent string
      /// </summary>
      /// <param name="url">Address of file</param>
      /// <returns>File as XDocument</returns>
      public static XDocument XDocLoadWithUserAgent(string url)
      {
         using (WebClient wc = new WebClient())
         {
            wc.Encoding = Encoding.UTF8;
            wc.Headers.Add("user-agent", GetUserAgentString());
            string data = wc.DownloadString(url);
            return XDocument.Load(new MemoryStream(Encoding.UTF8.GetBytes(data)));
         }
      }

   }
}
