namespace OnlyT.EventArgs
{
    using Models;
    using Newtonsoft.Json;

    public class TimerStartStopEventArgs : System.EventArgs
    {
        [JsonProperty(PropertyName = "talkId")]
        public int TalkId { get; set; }

        [JsonProperty(PropertyName = "command")]
        public StartStopTimerCommands Command { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "currentStatus")]
        public TimerStatus CurrentStatus { get; set; }
    }
}
