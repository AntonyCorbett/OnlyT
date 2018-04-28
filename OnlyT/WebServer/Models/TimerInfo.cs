namespace OnlyT.WebServer.Models
{
    using Newtonsoft.Json;

    internal class TimerInfo
    {
        [JsonProperty(PropertyName = "internalName")]
        public string InternalName { get; set; }

        [JsonProperty(PropertyName = "localisedTitle")]
        public string LocalisedTitle { get; set; }

        [JsonProperty(PropertyName = "status")]
        public TimerStatus Status { get; set; }

        [JsonProperty(PropertyName = "index")]
        public int Index { get; set; }

        [JsonProperty(PropertyName = "isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty(PropertyName = "isApplicable")]
        public bool? IsApplicable { get; set; }

        [JsonProperty(PropertyName = "isBellUsed")]
        public bool IsBellUsed { get; set; }

        [JsonProperty(PropertyName = "plannedAllocationSecs")]
        public int PlannedAllocationSecs { get; set; }

        [JsonProperty(PropertyName = "actualAllocationSecs")]
        public int ActualAllocationSecs { get; set; }

        [JsonProperty(PropertyName = "elapsedMillisecs")]
        public int ElapsedMillisecs { get; set; }

        [JsonProperty(PropertyName = "displayedMins")]
        public int DisplayedMins { get; set; }

        [JsonProperty(PropertyName = "displayedSecs")]
        public int DisplayedSecs { get; set; }

        [JsonProperty(PropertyName = "runningIndex")]
        public int RunningIndex { get; set; }
    }
}
