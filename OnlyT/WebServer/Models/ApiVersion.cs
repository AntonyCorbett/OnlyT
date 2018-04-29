namespace OnlyT.WebServer.Models
{
    using Newtonsoft.Json;

    public class ApiVersion
    {
        [JsonProperty(PropertyName = "lowVersion")]
        public int LowVersion { get; set; }

        [JsonProperty(PropertyName = "highVersion")]
        public int HighVersion { get; set; }
    }
}
