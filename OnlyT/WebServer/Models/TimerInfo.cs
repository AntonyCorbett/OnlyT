namespace OnlyT.WebServer.Models
{
    using Newtonsoft.Json;
    using OnlyT.Services.TalkSchedule;

    internal class TimerInfo
    {
        /// <summary>
        /// The talk identifier.
        /// </summary>
        /// <remarks><seealso cref="TalkTypesAutoMode"/></remarks>
        [JsonProperty(PropertyName = "talkId")]
        public int TalkId { get; set; }

        /// <summary>
        /// The talk title (as it appears in the UI).
        /// </summary>
        [JsonProperty(PropertyName = "talkTitle")]
        public string TalkTitle { get; set; }

        /// <summary>
        /// The original duration of the talk (before any manual or adaptive modifications).
        /// </summary>
        [JsonProperty(PropertyName = "originalDurationSecs")]
        public int OriginalDurationSecs { get; set; }

        /// <summary>
        /// The talk duration as modified by the user (or null if not modified).
        /// </summary>
        [JsonProperty(PropertyName = "modifiedDurationSecs")]
        public int? ModifiedDurationSecs { get; set; }

        /// <summary>
        /// The talk's "adapted" duration (only available after teh timer is started). Can be null
        /// if no adaption is made.
        /// </summary>
        [JsonProperty(PropertyName = "adaptedDurationSecs")]
        public int? AdaptedDurationSecs { get; set; }

        /// <summary>
        /// The talk's duration as used by the timer. This is the original duration
        /// unless modified by the user or overriden by the adaptive timer mechanism.
        /// </summary>
        [JsonProperty(PropertyName = "actualDurationSecs")]
        public int ActualDurationSecs { get; set; }

        /// <summary>
        /// Should the bell be used if the talk goes overtime?
        /// </summary>
        [JsonProperty(PropertyName = "usesBell")]
        public bool UsesBell { get; set; }

        [JsonProperty(PropertyName = "completedTimeSecs")]
        public int? CompletedTimeSecs { get; set; }

        [JsonProperty(PropertyName = "countUp")]
        public bool CountUp { get; set; }
    }
}
