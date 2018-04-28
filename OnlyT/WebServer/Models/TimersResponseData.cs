namespace OnlyT.WebServer.Models
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    internal class TimersResponseData
    {
        private readonly List<TimerInfo> _timerInfoCollection;

        [JsonProperty(PropertyName = "timerInfo")]
        public IEnumerable<TimerInfo> TimerInfo => _timerInfoCollection;

        public TimersResponseData()
        {
            _timerInfoCollection = new List<TimerInfo>();
        }

        public void Add(TimerInfo timerInfo)
        {
            _timerInfoCollection.Add(timerInfo);
        }
    }
}
