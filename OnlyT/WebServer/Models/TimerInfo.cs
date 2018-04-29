namespace OnlyT.WebServer.Models
{
    using Newtonsoft.Json;

    internal class TimerInfo
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "localisedTitle")]
        public string LocalisedTitle { get; set; }

        [JsonProperty(PropertyName = "originalDurationSecs")]
        public int OriginalDurationSecs { get; set; }

        [JsonProperty(PropertyName = "modifiedDurationSecs")]
        public int? ModifiedDurationSecs { get; set; }

        [JsonProperty(PropertyName = "adaptedDurationSecs")]
        public int? AdaptedDurationSecs { get; set; }

        [JsonProperty(PropertyName = "actualDurationSecs")]
        public int ActualDurationSecs { get; set; }

        [JsonProperty(PropertyName = "usesBell")]
        public bool UsesBell { get; set; }



        [JsonProperty(PropertyName = "status")]
        public TimerStatus Status { get; set; }

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
