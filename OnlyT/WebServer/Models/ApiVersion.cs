using Newtonsoft.Json;

namespace OnlyT.WebServer.Models
{
    public class ApiVersion
    {
        [JsonProperty(PropertyName = "lowVersion")]
        public int LowVersion { get; set; }

        [JsonProperty(PropertyName = "highVersion")]
        public int HighVersion { get; set; }
    }
}
