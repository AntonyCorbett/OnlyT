namespace OnlyT.MeetingTalkTimesFeed
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using Serilog;
    using Utils;

    internal class TimesFeed
    {
        private static readonly string FeedUrl = @"https://soundbox.blob.core.windows.net/meeting-feeds/feed.json";
        private readonly string _localFeedFile;
        private readonly int _tooOldDays = 20;
        private IEnumerable<Meeting> _meetingData;

        public TimesFeed()
        {
            _localFeedFile = Path.Combine(FileUtils.GetAppDataFolder(), "feed.json");
        }

        public Meeting GetMeetingDataForToday()
        {
            LoadFile();
            return _meetingData?.FirstOrDefault(x => x.Date.Date.Equals(DateUtils.GetMondayOfThisWeek()));
        }

        private bool LocalFileToOld()
        {
            bool tooOld = true;

            if (File.Exists(_localFeedFile))
            {
                var created = File.GetLastWriteTime(_localFeedFile);
                var diff = DateTime.Now - created;
                if (diff.TotalDays <= _tooOldDays)
                {
                    tooOld = false;
                }
            }

            return tooOld;
        }

        private void LoadFile()
        {
            if (_meetingData == null)
            {
                _meetingData = LoadFileInternal();
            }
        }

        private IReadOnlyCollection<Meeting> LoadFileInternal()
        {
            List<Meeting> result = null;

            var needRefresh = LocalFileToOld();

            if (!needRefresh)
            {
                try
                {
                    result = JsonConvert.DeserializeObject<List<Meeting>>(File.ReadAllText(_localFeedFile));
                    if (result == null || !result.Any())
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
                    result = JsonConvert.DeserializeObject<List<Meeting>>(content);
                    File.WriteAllText(_localFeedFile, content, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Downloading meeting feed");
                }
            }
            
            return result;
        }
    }
}
