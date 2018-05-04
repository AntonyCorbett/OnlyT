namespace OnlyT.MeetingTalkTimesFeed
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;

    internal class Meeting
    {
        [JsonProperty]
        public DateTime Date { get; set; }

        [JsonProperty]
        public List<TalkTimer> Talks { get; }

        public Meeting()
        {
            Talks = new List<TalkTimer>();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.Append(Date.ToShortDateString());
            foreach (var talk in Talks)
            {
                sb.Append(", ");
                sb.Append(talk);
            }

            return sb.ToString();
        }
    }
}
