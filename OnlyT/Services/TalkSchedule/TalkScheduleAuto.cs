namespace OnlyT.Services.TalkSchedule
{
    //// sample midweek meeting times (used to determine values 
    //// for StartOffsetIntoMeeting). From Jan 2020
    ////
    ////    7:00:00 (00:00) Start, song / prayer
    ////    7:05:00 (05:00) Opening comments
    ////    7:06:20 (06:20) Talk(10 mins)
    ////    7:16:40 (16:40) Digging(10 mins)
    ////    7:27:00 (27:00) Reading(4 mins)
    ////    7:31:20 (31:20) Counsel
    ////    7:32:40 (32:40) Prepare presentations(15 mins)
    ////    7:48:20 Song
    ////    7:51:40 (51:40) Part 1 (15 mins)
    ////    8:07:00 (67:00) Cong study(30 mins)
    ////    8:37:00 (97:00) Concluding comments(3 mins)
    ////    8:40:00 Song / prayer
    ////    7:00:00 (00:00) Start, song / prayer (19:00 - 19:05)
    ////    7:05:00 (05:00) Opening comments (19:05 - 19:08)
    ////    7:08:20 (08:20) Talk (10 mins) (19:08:20 - 19:18:20)
    ////    7:18:40 (18:40) Digging(8 mins) (19:18:40 - 19:26:40)
    ////    7:27:00 (27:00) Reading(4 mins) (19:27 - 19:31)
    ////    7:31:10 (31:10) Counsel (19:31:10 - 19:32:10)
    ////    7:32:20 (32:20) Initial call (2 mins) (19:32:20 - 
    ////    7:34:30 (34:30) Counsel
    ////    7:35:40 (35:40) Return visit (4 mins)
    ////    7:39:50 (39:50) Counsel
    ////    7:41:00 (41:00) Bible study or talk (6 mins)
    ////    7:47:10 (47:10) Counsel
    ////    7:48:20 Song
    ////    7:51:40 (51:40) Part 1 (15 mins)
    ////    8:07:00 (67:00) Cong study(30 mins)
    ////    8:37:00 (97:00) Concluding comments(3 mins)
    ////    8:40:00 Song / prayer
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using MeetingTalkTimesFeed;
    using Models;
    using OnlyT.Common.Services.DateTime;
    using Options;

    /// <summary>
    /// The talk schedule when in "Automatic" operating mode
    /// </summary>
    internal static class TalkScheduleAuto
    {
        // midweek meeting sections.
        private const string SectionTreasures = "Treasures";
        private const string SectionMinistry = "Ministry";
        private const string SectionLiving = "Living";

        // weekend sections.
        private const string SectionWeekend = "Weekend";

        public static bool SuccessGettingAutoFeedForMidWeekMtg { get; private set; }

        /// <summary>
        /// Gets the talk schedule.
        /// </summary>
        /// <param name="optionsService">Options service.</param>
        /// <param name="dateTimeService">DateTime service.</param>
        /// <param name="feedUri">The Uri of the json feed containing meeting schedule data.</param>
        /// <param name="isJanuary2020OrLater">True if current date is greater than 5 Jan 2020.</param>
        /// <returns>A collection of TalkScheduleItem.</returns>
        public static List<TalkScheduleItem> Read(
            IOptionsService optionsService,
            IDateTimeService dateTimeService,
            string feedUri,
            bool isJanuary2020OrLater)
        {
            var isCircuitVisit = optionsService.Options.IsCircuitVisit;
            
            return optionsService.Options.MidWeekOrWeekend == MidWeekOrWeekend.Weekend
                ? GetWeekendMeetingSchedule(isCircuitVisit)
                : GetMidweekMeetingSchedule(
                    isCircuitVisit, 
                    optionsService.Options.IsBellEnabled && optionsService.Options.AutoBell,
                    isJanuary2020OrLater,
                    GetFeedForToday(dateTimeService, feedUri));
        }

        public static List<TalkScheduleItem> GetMidweekScheduleForTesting(
            DateTime theDate,
            bool isJanuary2020OrLater)
        {
            return GetMidweekMeetingSchedule(
                false,
                false,
                isJanuary2020OrLater,
                TimesFeed.GetSampleMidweekMeetingDataForTesting(theDate));
        }
        
        private static Meeting? GetFeedForToday(IDateTimeService dateTimeService, string feedUri)
        {
            var feed = new TimesFeed(feedUri).GetMeetingDataForToday(dateTimeService);
            if (feed != null)
            {
                SuccessGettingAutoFeedForMidWeekMtg = true;
            }

            return feed;
        }

        private static TalkScheduleItem CreateTreasuresItem(
            TalkTypesAutoMode talkType, 
            string talkName,
            TimeSpan startOffset,
            TimeSpan duration,
            bool isStudentTalk,
            bool useBell,
            bool autoBell,
            bool persistFinalTimerValue)
        {
            return new TalkScheduleItem(talkType, talkName, SectionTreasures, Properties.Resources.SECTION_TREASURES)
            {
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = duration,
                BellApplicable = useBell,
                AutoBell = autoBell,
                IsStudentTalk = isStudentTalk,
                PersistFinalTimerValue = persistFinalTimerValue,
                Editable = true
            };
        }

        private static List<TalkScheduleItem> GetTreasuresSchedule(
            bool autoBell, bool isJanuary2020OrLater)
        {
            if (!isJanuary2020OrLater)
            {
                return GetTreasuresSchedulePreJan2020(autoBell);
            }

            return
            [
                CreateTreasuresItem(
                    TalkTypesAutoMode.OpeningComments,
                    Properties.Resources.TALK_OPENING_COMMENTS,
                    new TimeSpan(0, 5, 0),
                    TimeSpan.FromMinutes(1),
                    false,
                    false,
                    autoBell,
                    false),

                CreateTreasuresItem(
                    TalkTypesAutoMode.TreasuresTalk,
                    Properties.Resources.TALK_TREASURES,
                    new TimeSpan(0, 6, 20),
                    TimeSpan.FromMinutes(10),
                    false,
                    false,
                    autoBell,
                    false),

                CreateTreasuresItem(
                    TalkTypesAutoMode.DiggingTalk,
                    Properties.Resources.TALK_DIGGING,
                    new TimeSpan(0, 16, 40),
                    TimeSpan.FromMinutes(10),
                    false,
                    false,
                    autoBell,
                    false),

                CreateTreasuresItem(
                    TalkTypesAutoMode.Reading,
                    Properties.Resources.TALK_READING,
                    new TimeSpan(0, 27, 0),
                    TimeSpan.FromMinutes(4),
                    true,
                    true,
                    autoBell,
                    true)
            ];
        }

        private static List<TalkScheduleItem> GetTreasuresSchedulePreJan2020(bool autoBell)
        {
            return
            [
                CreateTreasuresItem(
                    TalkTypesAutoMode.OpeningComments,
                    Properties.Resources.TALK_OPENING_COMMENTS,
                    new TimeSpan(0, 5, 0),
                    TimeSpan.FromMinutes(3),
                    false,
                    false,
                    autoBell,
                    false),

                CreateTreasuresItem(
                    TalkTypesAutoMode.TreasuresTalk,
                    Properties.Resources.TALK_TREASURES,
                    new TimeSpan(0, 8, 20),
                    TimeSpan.FromMinutes(10),
                    false,
                    false,
                    autoBell,
                    false),

                CreateTreasuresItem(
                    TalkTypesAutoMode.DiggingTalk,
                    Properties.Resources.TALK_DIGGING,
                    new TimeSpan(0, 18, 40),
                    TimeSpan.FromMinutes(8),
                    false,
                    false,
                    autoBell,
                    false),

                CreateTreasuresItem(
                    TalkTypesAutoMode.Reading,
                    Properties.Resources.TALK_READING,
                    new TimeSpan(0, 27, 0),
                    TimeSpan.FromMinutes(4),
                    true,
                    true,
                    autoBell,
                    true)
            ];
        }

        private static TalkScheduleItem CreateMinistryItem(
            TalkTypesAutoMode talkType,
            string talkName,
            TimeSpan startOffset,
            TimeSpan duration,
            bool isStudentTalk,
            bool useBell,
            bool autoBell,
            bool persistFinalTimerValue,
            bool editableTime)
        {
            return new TalkScheduleItem(talkType, talkName, SectionMinistry, Properties.Resources.SECTION_MINISTRY)
            {
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = duration,
                BellApplicable = useBell,
                AutoBell = autoBell,
                PersistFinalTimerValue = persistFinalTimerValue,
                Editable = editableTime,
                IsStudentTalk = isStudentTalk
            };
        }

        private static List<TalkScheduleItem> GetMinistrySchedule(Meeting? meetingData, bool autoBell)
        {
            var result = new List<TalkScheduleItem>();

            var timers = new List<TalkTimer>();

            const int maxItems = 4;

            for (int n = 0; n < maxItems; ++n)
            {
                var talkType = TalkTypesUtils.GetMinistryTalkType(n);

                TalkTimer? item;

                if (meetingData == null)
                {
                    // failed to download auto schedule!
                    item = new TalkTimer
                    {
                        IsStudentTalk = true,
                        Minutes = 3, // as good a guess as any
                        TalkType = talkType
                    };
                }
                else
                {
                    item = meetingData.Talks.FirstOrDefault(x => x.TalkType.Equals(talkType));
                }

                if (item != null)
                {
                    timers.Add(item);
                }
            }

            var startOffset = new TimeSpan(0, 32, 20);

            for (var n = 0; n < timers.Count; ++n)
            {
                var talkType = TalkTypesUtils.GetAutoModeMinistryTalkType(n);
                var timer = timers[n];

                result.Add(CreateMinistryItem(
                    talkType,
                    GetMinistryItemTitle(n + 1),
                    startOffset,
                    TimeSpan.FromMinutes(timer.Minutes),
                    timer.IsStudentTalk,
                    timer.IsStudentTalk,
                    autoBell,
                    timer.IsStudentTalk,
                    true));

                startOffset = startOffset.Add(TimeSpan.FromMinutes(timer.Minutes));
                if (timer.IsStudentTalk)
                {
                    // counsel...
                    startOffset = startOffset.Add(TimeSpan.FromMinutes(1));
                }

                startOffset = startOffset.Add(TimeSpan.FromSeconds(20));
            }

            return result;
        }

        private static string GetMinistryItemTitle(int item)
        {
            return item switch
            {
                1 => Properties.Resources.MINISTRY1,
                2 => Properties.Resources.MINISTRY2,
                3 => Properties.Resources.MINISTRY3,
                4 => Properties.Resources.MINISTRY4,
                
                // ReSharper disable once LocalizableElement
                _ => throw new ArgumentException("Unknown item", nameof(item))
            };
        }

        private static TalkScheduleItem CreateLivingItem(
            TalkTypesAutoMode talkType,
            string talkName,
            TimeSpan startOffset,
            TimeSpan duration)
        {
            return new TalkScheduleItem(talkType, talkName, SectionLiving, Properties.Resources.SECTION_LIVING)
            {
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = duration,
                Editable = true,
                AllowAdaptive = true
            };
        }

        private static List<TalkScheduleItem> GetLivingSchedule(bool isCircuitVisit, Meeting? meetingData)
        {
            var result = new List<TalkScheduleItem>();

            var timerPart1 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Living1)) ??
                             new TalkTimer { Minutes = 15, TalkType = TalkTypes.Living1 };

            var timerPart2 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Living2));

            result.Add(CreateLivingItem(
                TalkTypesAutoMode.LivingPart1,
                Properties.Resources.TALK_LIVING1,
                new TimeSpan(0, 51, 40),
                TimeSpan.FromMinutes(timerPart1.Minutes)));

            result.Add(CreateLivingItem(
                TalkTypesAutoMode.LivingPart2,
                Properties.Resources.TALK_LIVING2,
                new TimeSpan(0, 51, 40).Add(TimeSpan.FromMinutes(timerPart1.Minutes)),
                TimeSpan.FromMinutes(timerPart2?.Minutes ?? 0)));

            if (isCircuitVisit)
            {
                result.Add(CreateLivingItem(
                    TalkTypesAutoMode.ConcludingComments,
                    Properties.Resources.TALK_CONCLUDING_COMMENTS,
                    new TimeSpan(1, 7, 0),
                    TimeSpan.FromMinutes(3)));

                result.Add(CreateLivingItem(
                    TalkTypesAutoMode.CircuitServiceTalk,
                    Properties.Resources.TALK_SERVICE,
                    new TimeSpan(1, 10, 0),
                    TimeSpan.FromMinutes(30)));
            }
            else
            {
                result.Add(CreateLivingItem(
                    TalkTypesAutoMode.CongBibleStudy,
                    Properties.Resources.TALK_CONG_STUDY,
                    new TimeSpan(1, 7, 0),
                    TimeSpan.FromMinutes(30)));

                result.Add(CreateLivingItem(
                    TalkTypesAutoMode.ConcludingComments,
                    Properties.Resources.TALK_CONCLUDING_COMMENTS,
                    new TimeSpan(1, 37, 0),
                    TimeSpan.FromMinutes(3)));
            }

            return result;
        }

        private static List<TalkScheduleItem> GetMidweekMeetingSchedule(
            bool isCircuitVisit, 
            bool autoBell,
            bool isJanuary2020OrLater,
            Meeting? meetingData)
        {
            var result = new List<TalkScheduleItem>();

            // Treasures...
            result.AddRange(GetTreasuresSchedule(autoBell, isJanuary2020OrLater));

            // Ministry...
            result.AddRange(GetMinistrySchedule(meetingData, autoBell));

            // Living...
            result.AddRange(GetLivingSchedule(isCircuitVisit, meetingData));

            return result;
        }

        private static TalkScheduleItem CreateWeekendItem(
            TalkTypesAutoMode talkType,
            string talkName,
            TimeSpan startOffset,
            TimeSpan duration,
            bool allowAdaptive)
        {
            return new TalkScheduleItem(talkType, talkName, SectionWeekend, Properties.Resources.SECTION_WEEKEND)
            {
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = duration,
                Editable = true,
                AllowAdaptive = allowAdaptive
            };
        }

        private static List<TalkScheduleItem> GetWeekendMeetingSchedule(bool isCircuitVisit)
        {
            var result = new List<TalkScheduleItem>();

            if (isCircuitVisit)
            {
                result.Add(CreateWeekendItem(
                    TalkTypesAutoMode.PublicTalk,
                    Properties.Resources.TALK_PUBLIC,
                    new TimeSpan(0, 5, 0),
                    TimeSpan.FromMinutes(30),
                    false));

                // song here
                result.Add(CreateWeekendItem(
                    TalkTypesAutoMode.Watchtower,
                    Properties.Resources.TALK_WT,
                    new TimeSpan(0, 40, 0),
                    TimeSpan.FromMinutes(30),
                    true));

                result.Add(CreateWeekendItem(
                    TalkTypesAutoMode.CircuitServiceTalk,
                    Properties.Resources.TALK_CONCLUDING,
                    new TimeSpan(0, 70, 0),
                    TimeSpan.FromMinutes(30),
                    true));
            }
            else
            {
                result.Add(CreateWeekendItem(
                    TalkTypesAutoMode.PublicTalk,
                    Properties.Resources.TALK_PUBLIC,
                    new TimeSpan(0, 5, 0),
                    TimeSpan.FromMinutes(30),
                    false));
                
                // song
                result.Add(CreateWeekendItem(
                    TalkTypesAutoMode.Watchtower,
                    Properties.Resources.TALK_WT,
                    new TimeSpan(0, 40, 0),
                    TimeSpan.FromMinutes(60),
                    true));
            }
            
            return result;
        }
    }
}