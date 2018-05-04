namespace OnlyT.MeetingTalkTimesFeed
{
    using Newtonsoft.Json;

    internal class TalkTimer
    {
        [JsonProperty]
        public TalkTypes TalkType { get; set; }

        [JsonProperty]
        public int Minutes { get; set; }

        [JsonProperty]
        public bool IsStudentTalk { get; set; }

        public override string ToString()
        {
            var studentTalkeStr = IsStudentTalk ? " (student)" : string.Empty;
            return $"{TalkType}, {Minutes} mins{studentTalkeStr}";
        }
    }
}
