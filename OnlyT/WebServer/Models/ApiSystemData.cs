using Newtonsoft.Json;

namespace OnlyT.WebServer.Models
{
    public class ApiSystemData
    {
        [JsonProperty(PropertyName = "machineName")]
        public string MachineName { get; set; }

        [JsonProperty(PropertyName = "accountName")]
        public string AccountName { get; set; }

        [JsonProperty(PropertyName = "onlyTVersion")]
        public string OnlyTVersion { get; set; }

        [JsonProperty(PropertyName = "apiVersion")]
        public ApiVersion ApiVersion { get; set; }

        [JsonProperty(PropertyName = "culture")]
        public ApiCultureData Culture { get; set; }

        [JsonProperty(PropertyName = "workingSet")]
        public long WorkingSet { get; set; }

        [JsonProperty(PropertyName = "sessionId")]
        public string SessionId { get; set; }

        [JsonProperty(PropertyName = "apiEnabled")]
        public bool ApiEnabled { get; set; }

        [JsonProperty(PropertyName = "apiCodeRequired")]
        public bool ApiCodeRequired { get; set; }
    }
}
