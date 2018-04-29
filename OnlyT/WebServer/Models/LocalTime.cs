namespace OnlyT.WebServer.Models
{
    using Newtonsoft.Json;

    public class LocalTime
    {
        [JsonProperty(PropertyName = "year")]
        public int Year { get; set; }

        [JsonProperty(PropertyName = "month")]
        public int Month { get; set; }

        [JsonProperty(PropertyName = "day")]
        public int Day { get; set; }

        [JsonProperty(PropertyName = "hour")]
        public int Hour { get; set; }

        [JsonProperty(PropertyName = "min")]
        public int Min { get; set; }

        [JsonProperty(PropertyName = "second")]
        public int Second { get; set; }
    }
}
