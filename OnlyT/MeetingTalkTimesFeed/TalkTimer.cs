using Newtonsoft.Json;

namespace OnlyT.MeetingTalkTimesFeed;

internal sealed class TalkTimer
{
    [JsonProperty]
    public TalkTypes TalkType { get; set; }

    [JsonProperty]
    public int Minutes { get; set; }

    [JsonProperty]
    public bool IsStudentTalk { get; set; }

    public override string ToString()
    {
        var studentTalkStr = IsStudentTalk ? " (student)" : string.Empty;
        return $"{TalkType}, {Minutes} mins{studentTalkStr}";
    }
}