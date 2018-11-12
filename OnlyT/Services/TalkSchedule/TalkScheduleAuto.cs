namespace OnlyT.Services.TalkSchedule
{
    //// sample midweek meeting times (used to determine values 
    //// for StartOffsetIntoMeeting)
    ////
    ////    7:00:00 (00:00) Start, song / prayer
    ////    7:05:00 (05:00) Opening comments
    ////    7:08:20 (08:20) Talk(10 mins)
    ////    7:18:40 (18:40) Digging(8 mins)
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

        /// <summary>
        /// Gets the talk schedule.
        /// </summary>
        /// <param name="optionsService">Options service.</param>
        /// <returns>A collection of TalkScheduleItem.</returns>
        public static IEnumerable<TalkScheduleItem> Read(IOptionsService optionsService)
        {
            var isCircuitVisit = optionsService.Options.IsCircuitVisit;
            
            return optionsService.Options.MidWeekOrWeekend == MidWeekOrWeekend.Weekend
                ? GetWeekendMeetingSchedule(isCircuitVisit)
                : GetMidweekMeetingSchedule(isCircuitVisit, new TimesFeed().GetMeetingDataForToday());
        }

        private static TalkScheduleItem CreateTreasuresItem(
            TalkTypesAutoMode talkType, 
            string talkName,
            TimeSpan startOffset,
            TimeSpan duration,
            bool isStudentTalk = false,
            bool useBell = false,
            bool persistFinalTimerValue = false)
        {
            return new TalkScheduleItem(talkType)
            {
                Name = talkName,
                MeetingSectionNameLocalised = Properties.Resources.SECTION_TREASURES,
                MeetingSectionNameInternal = SectionTreasures,
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = duration,
                Bell = useBell,
                IsStudentTalk = isStudentTalk,
                PersistFinalTimerValue = persistFinalTimerValue
            };
        }

        private static List<TalkScheduleItem> GetTreasuresSchedule()
        {
            return new List<TalkScheduleItem>
            {
                CreateTreasuresItem(TalkTypesAutoMode.OpeningComments, Properties.Resources.TALK_OPENING_COMMENTS, new TimeSpan(0, 5, 0), TimeSpan.FromMinutes(3)),
                CreateTreasuresItem(TalkTypesAutoMode.TreasuresTalk, Properties.Resources.TALK_TREASURES, new TimeSpan(0, 8, 20), TimeSpan.FromMinutes(10)),
                CreateTreasuresItem(TalkTypesAutoMode.DiggingTalk, Properties.Resources.TALK_DIGGING, new TimeSpan(0, 18, 40), TimeSpan.FromMinutes(8)),
                CreateTreasuresItem(TalkTypesAutoMode.Reading, Properties.Resources.TALK_READING, new TimeSpan(0, 27, 0), TimeSpan.FromMinutes(4), true, true, true)
            };
        }

        private static TalkTimer CreateDefaultMinistryTalkTimer(TalkTypes talkType)
        {
            return new TalkTimer
            {
                Minutes = 4,
                IsStudentTalk = false,
                TalkType = talkType,
            };
        }

        private static TalkScheduleItem CreateMinistryItem(
            TalkTypesAutoMode talkType,
            string talkName,
            TimeSpan startOffset,
            TimeSpan duration,
            bool isStudentTalk = false,
            bool useBell = false,
            bool persistFinalTimerValue = false,
            bool editableTime = false)
        {
            return new TalkScheduleItem(talkType)
            {
                Name = talkName,
                MeetingSectionNameLocalised = Properties.Resources.SECTION_MINISTRY,
                MeetingSectionNameInternal = SectionMinistry,
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = duration,
                Bell = useBell,
                PersistFinalTimerValue = persistFinalTimerValue,
                Editable = editableTime,
                IsStudentTalk = isStudentTalk
            };
        }

        private static IEnumerable<TalkScheduleItem> GetMinistrySchedule(Meeting meetingData)
        {
            var result = new List<TalkScheduleItem>();

            TalkTimer timerItem1 = null;
            TalkTimer timerItem2 = null;
            TalkTimer timerItem3 = null;
            TalkTimer timerItem4 = null;

            if (meetingData != null)
            {
                if (meetingData.Date.Year < 2019)
                {
                    timerItem1 =
                        meetingData.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Pre2019Ministry1)) ??
                        CreateDefaultMinistryTalkTimer(TalkTypes.Pre2019Ministry1);
                    timerItem2 =
                        meetingData.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Pre2019Ministry2)) ??
                        CreateDefaultMinistryTalkTimer(TalkTypes.Pre2019Ministry2);
                    timerItem3 =
                        meetingData.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Pre2019Ministry3)) ??
                        CreateDefaultMinistryTalkTimer(TalkTypes.Pre2019Ministry3);
                }
                else
                {
                    timerItem1 =
                        meetingData.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Ministry1)) ??
                        CreateDefaultMinistryTalkTimer(TalkTypes.Ministry1);
                    timerItem2 =
                        meetingData.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Ministry2)) ??
                        CreateDefaultMinistryTalkTimer(TalkTypes.Ministry2);
                    timerItem3 =
                        meetingData.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Ministry3)) ??
                        CreateDefaultMinistryTalkTimer(TalkTypes.Ministry3);
                    timerItem4 =
                        meetingData.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Ministry4)) ??
                        CreateDefaultMinistryTalkTimer(TalkTypes.Ministry4);
                }
            }

            TimeSpan startOffset = new TimeSpan(0, 32, 20);

            if (timerItem1 != null)
            {
                result.Add(CreateMinistryItem(
                    TalkTypesAutoMode.MinistryItem1,
                    GetMinistryItemTitle(1),
                    startOffset,
                    TimeSpan.FromMinutes(timerItem1.Minutes),
                    timerItem1.IsStudentTalk,
                    timerItem1.IsStudentTalk,
                    timerItem1.IsStudentTalk,
                    true));

                startOffset = startOffset.Add(TimeSpan.FromMinutes(timerItem1.Minutes));
                if (timerItem1.IsStudentTalk)
                {
                    // counsel...
                    startOffset = startOffset.Add(TimeSpan.FromMinutes(1));
                }

                startOffset = startOffset.Add(TimeSpan.FromSeconds(20));
            }

            if (timerItem2 != null)
            {
                result.Add(CreateMinistryItem(
                    TalkTypesAutoMode.MinistryItem2,
                    GetMinistryItemTitle(2),
                    startOffset,
                    TimeSpan.FromMinutes(timerItem2.Minutes),
                    timerItem2.IsStudentTalk,
                    timerItem2.IsStudentTalk,
                    timerItem2.IsStudentTalk,
                    true));

                startOffset = startOffset.Add(TimeSpan.FromMinutes(timerItem2.Minutes));
                if (timerItem2.IsStudentTalk)
                {
                    // counsel...
                    startOffset = startOffset.Add(TimeSpan.FromMinutes(1));
                }

                startOffset = startOffset.Add(TimeSpan.FromSeconds(20));
            }

            if (timerItem3 != null)
            {
                result.Add(CreateMinistryItem(
                    TalkTypesAutoMode.MinistryItem3,
                    GetMinistryItemTitle(3),
                    startOffset,
                    TimeSpan.FromMinutes(timerItem3.Minutes),
                    timerItem3.IsStudentTalk,
                    timerItem3.IsStudentTalk,
                    timerItem3.IsStudentTalk,
                    true));

                startOffset = startOffset.Add(TimeSpan.FromMinutes(timerItem3.Minutes));
                if (timerItem3.IsStudentTalk)
                {
                    // counsel...
                    startOffset = startOffset.Add(TimeSpan.FromMinutes(1));
                }

                startOffset = startOffset.Add(TimeSpan.FromSeconds(20));
            }

            if (timerItem4 != null)
            {
                result.Add(CreateMinistryItem(
                    TalkTypesAutoMode.MinistryItem4,
                    GetMinistryItemTitle(4),
                    startOffset,
                    TimeSpan.FromMinutes(timerItem4.Minutes),
                    timerItem4.IsStudentTalk,
                    timerItem4.IsStudentTalk,
                    timerItem4.IsStudentTalk,
                    true));
            }

            return result;
        }

        private static string GetMinistryItemTitle(int item)
        {
            switch (item)
            {
                case 1:
                    return Properties.Resources.MINISTRY1;
                case 2:
                    return Properties.Resources.MINISTRY2;
                case 3:
                    return Properties.Resources.MINISTRY3;
                case 4:
                    return Properties.Resources.MINISTRY4;
            }

            throw new ArgumentException(@"Unknown item", nameof(item));
        }

        private static TalkScheduleItem CreateLivingItem(
            TalkTypesAutoMode talkType,
            string talkName,
            TimeSpan startOffset,
            TimeSpan duration)
        {
            return new TalkScheduleItem(talkType)
            {
                Name = talkName,
                MeetingSectionNameLocalised = Properties.Resources.SECTION_LIVING,
                MeetingSectionNameInternal = SectionLiving,
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = duration,
                Editable = true,
                AllowAdaptive = true
            };
        }

        private static IEnumerable<TalkScheduleItem> GetLivingSchedule(bool isCircuitVisit, Meeting meetingData)
        {
            var result = new List<TalkScheduleItem>();

            TalkTimer timerPart1 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Pre2019Living1)) ??
                                   new TalkTimer { Minutes = 15, TalkType = TalkTypes.Pre2019Living1 };

            TalkTimer timerPart2 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Pre2019Living2));

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

        private static List<TalkScheduleItem> GetMidweekMeetingSchedule(bool isCircuitVisit, Meeting meetingData)
        {
            var result = new List<TalkScheduleItem>();

            // Treasures...
            result.AddRange(GetTreasuresSchedule());

            // Ministry...
            result.AddRange(GetMinistrySchedule(meetingData));

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
            return new TalkScheduleItem(talkType)
            {
                Name = talkName,
                MeetingSectionNameLocalised = Properties.Resources.SECTION_WEEKEND,
                MeetingSectionNameInternal = SectionWeekend,
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