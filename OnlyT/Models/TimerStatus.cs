using System;
using Newtonsoft.Json;

namespace OnlyT.Models;

public class TimerStatus
{
    [JsonProperty(PropertyName = "talkId")]
    public int? TalkId { get; set; }

    [JsonProperty(PropertyName = "targetSeconds")]
    public int TargetSeconds { get; set; }

    [JsonProperty(PropertyName = "isRunning")]
    public bool IsRunning { get; set; }

    [JsonProperty(PropertyName = "timeElapsed")]
    public TimeSpan TimeElapsed { get; set; }

    [JsonProperty(PropertyName = "closingSecs")]
    public int ClosingSecs { get; set; }
}