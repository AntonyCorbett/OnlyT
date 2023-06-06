using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OnlyT.Common.Services.DateTime;
using Serilog;
using OnlyT.Utils;
using OnlyT.EventTracking;

namespace OnlyT.MeetingTalkTimesFeed;

internal sealed class TimesFeed
{
#pragma warning disable S1075 // URIs should not be hardcoded
    private static readonly string FeedUrl = "https://soundbox.blob.core.windows.net/meeting-feeds/feed.json";
#pragma warning restore S1075 // URIs should not be hardcoded

    private readonly string _localFeedFile;
    private readonly int _tooOldDays = 20;
    private IEnumerable<Meeting>? _meetingData;

    public TimesFeed()
    {
        _localFeedFile = Path.Combine(FileUtils.GetAppDataFolder(), "feed.json");
    }

    public Meeting? GetMeetingDataForToday(IDateTimeService dateTimeService)
    {
        LoadFile(dateTimeService.Now().Date);
        return GetMeetingDataForTodayInternal(_meetingData);
    }

    public static Meeting GetSampleMidweekMeetingDataForTesting(DateTime theDate)
    {
        var result = new Meeting { Date = theDate };

        result.Talks.Add(new TalkTimer
        {
            TalkType = TalkTypes.Ministry1,
            IsStudentTalk = false,
            Minutes = 4
        });

        result.Talks.Add(new TalkTimer
        {
            TalkType = TalkTypes.Ministry2,
            IsStudentTalk = true,
            Minutes = 2
        });

        result.Talks.Add(new TalkTimer
        {
            TalkType = TalkTypes.Ministry3,
            IsStudentTalk = true,
            Minutes = 3
        });

        result.Talks.Add(new TalkTimer
        {
            TalkType = TalkTypes.Ministry4,
            IsStudentTalk = true,
            Minutes = 3
        });

        result.Talks.Add(new TalkTimer
        {
            TalkType = TalkTypes.Living1,
            Minutes = 15
        });

        return result;
    }

    private bool LocalFileTooOld(DateTime today)
    {
        var tooOld = true;

        if (File.Exists(_localFeedFile))
        {
            var created = File.GetLastWriteTime(_localFeedFile);
            var diff = today - created;
            if (diff.TotalDays <= _tooOldDays)
            {
                tooOld = false;
            }
        }

        return tooOld;
    }

    private void LoadFile(DateTime today)
    {
        _meetingData ??= LoadFileInternal(today);
    }

    private IReadOnlyCollection<Meeting>? LoadFileInternal(DateTime today)
    {
        List<Meeting>? result = null;

        var needRefresh = LocalFileTooOld(today);

        if (!needRefresh)
        {
            try
            {
                result = JsonConvert.DeserializeObject<List<Meeting>>(File.ReadAllText(_localFeedFile));
                if (result == null || result.Count == 0 || GetMeetingDataForTodayInternal(result) == null)
                {
                    needRefresh = true;
                }
            }
            catch (Exception ex)
            {
                needRefresh = true;                
                Log.Logger.Error(ex, "Getting meeting feed");
            }
        }

        if (needRefresh)
        {
            try
            {
                var content = WebUtils.LoadWithUserAgent(FeedUrl);
                if (content != null)
                {
                    result = JsonConvert.DeserializeObject<List<Meeting>>(content);
                    File.WriteAllText(_localFeedFile, content, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Downloading meeting feed");
            }
        }
            
        return result;
    }

    private static Meeting? GetMeetingDataForTodayInternal(IEnumerable<Meeting>? meetingData)
    {
        var monday = DateUtils.GetMondayOfThisWeek();
        return meetingData?.FirstOrDefault(x => x.Date.Date.Equals(monday));
    }
}