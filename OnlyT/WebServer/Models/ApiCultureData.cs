namespace OnlyT.WebServer.Models
{
    using Newtonsoft.Json;

    public class ApiCultureData
    {
        [JsonProperty(PropertyName = "name")]
        public string? Name { get; set; }

        [JsonProperty(PropertyName = "isoCode2")]
        public string? IsoCode2 { get; set; }

        [JsonProperty(PropertyName = "isoCode3")]
        public string? IsoCode3 { get; set; }
    }
}
