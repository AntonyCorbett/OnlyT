namespace OnlyT.WebServer.Models;

using System.Collections.Generic;
using System.Linq;
using ErrorHandling;
using Newtonsoft.Json;
using OnlyT.Models;
using Services.Options;
using Services.TalkSchedule;
using Services.Timer;

internal sealed class TimersResponseData
{
    public TimersResponseData(
        ITalkScheduleService talkService, 
        ITalkTimerService timerService,
        IOptionsService optionsService)
    {
        var talks = talkService.GetTalkScheduleItems();
        Status = timerService.GetStatus();

        TimerInfo = [];

        var countUpByDefault = optionsService.Options.CountUp;
        foreach (var talk in talks)
        {
            TimerInfo.Add(CreateTimerInfo(talk, countUpByDefault));
        }
    }

    public TimersResponseData(
        ITalkScheduleService talkService, 
        ITalkTimerService timerService, 
        IOptionsService optionsService,
        int talkId)
    {
        var talks = talkService.GetTalkScheduleItems();
        var talk = talks.SingleOrDefault(x => x.Id == talkId) ?? throw new WebServerException(WebServerErrorCode.TimerDoesNotExist);

        Status = timerService.GetStatus();
        TimerInfo = [CreateTimerInfo(talk, optionsService.Options.CountUp)];
    }

    [JsonProperty(PropertyName = "status")]
    public TimerStatus Status { get; }

    [JsonProperty(PropertyName = "timerInfo")]
    public List<TimerInfo> TimerInfo { get; }

    private static TimerInfo CreateTimerInfo(TalkScheduleItem talk, bool countUpByDefault)
    {
        return new TimerInfo
        {
            TalkId = talk.Id,
            TalkTitle = talk.Name,
            MeetingSectionNameInternal = talk.MeetingSectionNameInternal,
            MeetingSectionNameLocalised = talk.MeetingSectionNameLocalised,
            OriginalDurationSecs = (int)talk.OriginalDuration.TotalSeconds,
            ModifiedDurationSecs = talk.ModifiedDuration == null
                ? null
                : (int?)talk.ModifiedDuration.Value.TotalSeconds,
            AdaptedDurationSecs = talk.AdaptedDuration == null
                ? null
                : (int?)talk.AdaptedDuration.Value.TotalSeconds,
            ActualDurationSecs = (int)talk.ActualDuration.TotalSeconds,
            UsesBell = talk.BellApplicable,
            CompletedTimeSecs = talk.CompletedTimeSecs,
            CountUp = talk.CountUp ?? countUpByDefault,
            ClosingSecs = talk.ClosingSecs
        };
    }
}