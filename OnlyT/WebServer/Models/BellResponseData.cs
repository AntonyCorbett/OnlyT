namespace OnlyT.WebServer.Models;

using Newtonsoft.Json;

internal sealed class BellResponseData
{
    [JsonProperty(PropertyName = "success")]
    public bool Success { get; set; }
}