using Newtonsoft.Json;

namespace OnlyT.WebServer.Models;

internal sealed class TimerDurationChangeRequest
{
    [JsonProperty(PropertyName = "deltaSeconds")]
    public int DeltaSeconds { get; set; }
}
