using Newtonsoft.Json;

namespace OnlyT.EventArgs;

public class TimerDurationChangeEventArgs : System.EventArgs
{
    [JsonProperty(PropertyName = "talkId")]
    public int TalkId { get; set; }

    [JsonProperty(PropertyName = "deltaSeconds")]
    public int DeltaSeconds { get; set; }

    [JsonProperty(PropertyName = "success")]
    public bool Success { get; set; }

    [JsonProperty(PropertyName = "newDurationSecs")]
    public int NewDurationSecs { get; set; }
}
