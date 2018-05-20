namespace OnlyT.Models
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    using System;
    using Newtonsoft.Json;

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
    }
}
