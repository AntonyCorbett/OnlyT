namespace OnlyT.WebServer.Models
{
    using Newtonsoft.Json;

    internal class BellResponseData
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }
    }
}
