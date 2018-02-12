namespace OnlyT.MeetingTalkTimesFeed
{
    internal class TalkTimer
    {
        public TalkTypes TalkType { get; set; }

        public int Minutes { get; set; }

        public bool IsStudentTalk { get; set; }

        public override string ToString()
        {
            var studentTalkeStr = IsStudentTalk ? " (student)" : string.Empty;
            return $"{TalkType}, {Minutes} mins{studentTalkeStr}";
        }
    }
}
