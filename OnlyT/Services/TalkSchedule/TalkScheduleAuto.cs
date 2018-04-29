// sample midweek meeting times (used to determine values 
// for StartOffsetIntoMeeting)
//
//    7:00:00 (00:00) Start, song / prayer
//    7:05:00 (05:00) Opening comments
//    7:08:20 (08:20) Talk(10 mins)
//    7:18:40 (18:40) Digging(8 mins)
//    7:27:00 (27:00) Reading(4 mins)
//    7:31:20 (31:20) Counsel
//    7:32:40 (32:40) Prepare presentations(15 mins)
//    7:48:20 Song
//    7:51:40 (51:40) Part 1 (15 mins)
//    8:07:00 (67:00) Cong study(30 mins)
//    8:37:00 (97:00) Concluding comments(3 mins)
//    8:40:00 Song / prayer

//    7:00:00 (00:00) Start, song / prayer
//    7:05:00 (05:00) Opening comments
//    7:08:20 (08:20) Talk (10 mins)
//    7:18:40 (18:40) Digging(8 mins)
//    7:27:00 (27:00) Reading(4 mins)
//    7:31:10 (31:10) Counsel
//    7:32:20 (32:20) Initial call (2 mins)
//    7:34:30 (34:30) Counsel
//    7:35:40 (35:40) Return visit (4 mins)
//    7:39:50 (39:50) Counsel
//    7:41:00 (41:00) Bible study or talk (6 mins)
//    7:47:10 (47:10) Counsel
//    7:48:20 Song
//    7:51:40 (51:40) Part 1 (15 mins)
//    8:07:00 (67:00) Cong study(30 mins)
//    8:37:00 (97:00) Concluding comments(3 mins)
//    8:40:00 Song / prayer

namespace OnlyT.Services.TalkSchedule
{
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
        /// <summary>
        /// Gets the talk schedule.
        /// </summary>
        /// <param name="optionsService">Options service.</param>
        /// <returns>A collection of TalkScheduleItem.</returns>
        public static IEnumerable<TalkScheduleItem> Read(IOptionsService optionsService)
        {
            bool isCircuitVisit = optionsService.Options.IsCircuitVisit;
            
            return optionsService.Options.MidWeekOrWeekend == MidWeekOrWeekend.Weekend
               ? GetWeekendMeetingSchedule(isCircuitVisit)
               : GetMidweekMeetingSchedule(isCircuitVisit, new TimesFeed().GetTodaysMeetingData());
        }

        private static List<TalkScheduleItem> GetTreasuresSchedule()
        {
            return new List<TalkScheduleItem>
            {
                new TalkScheduleItem(TalkTypesAutoMode.OpeningComments)
                {
                    Name = Properties.Resources.TALK_OPENING_COMMENTS,
                    StartOffsetIntoMeeting = new TimeSpan(0, 5, 0),
                    OriginalDuration = TimeSpan.FromMinutes(3)
                },

                new TalkScheduleItem(TalkTypesAutoMode.TreasuresTalk)
                {   
                    Name = Properties.Resources.TALK_TREASURES,
                    StartOffsetIntoMeeting = new TimeSpan(0, 8, 20),
                    OriginalDuration = TimeSpan.FromMinutes(10)
                },

                new TalkScheduleItem(TalkTypesAutoMode.DiggingTalk)
                {
                    Name = Properties.Resources.TALK_DIGGING,
                    StartOffsetIntoMeeting = new TimeSpan(0, 18, 40),
                    OriginalDuration = TimeSpan.FromMinutes(8)
                },

                new TalkScheduleItem(TalkTypesAutoMode.Reading)
                {
                    Name = Properties.Resources.TALK_READING,
                    StartOffsetIntoMeeting = new TimeSpan(0, 27, 0),
                    OriginalDuration = TimeSpan.FromMinutes(4),
                    Bell = true
                }
            };
        }

        private static TalkTimer CreateDefaultMinistryTalkTimer(TalkTypes talkType)
        {
            return new TalkTimer
            {
                Minutes = 4,
                IsStudentTalk = false,
                TalkType = talkType
            };
        }

        private static IEnumerable<TalkScheduleItem> GetMinistrySchedule(Meeting meetingData)
        {
            var result = new List<TalkScheduleItem>();

            TalkTimer timerItem1 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Ministry1)) ?? CreateDefaultMinistryTalkTimer(TalkTypes.Ministry1);
            TalkTimer timerItem2 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Ministry2)) ?? CreateDefaultMinistryTalkTimer(TalkTypes.Ministry2);
            TalkTimer timerItem3 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Ministry3)) ?? CreateDefaultMinistryTalkTimer(TalkTypes.Ministry3);

            TimeSpan startOffset = new TimeSpan(0, 32, 20);

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.MinistryItem1)
            {
                Name = GetMinistryItemTitle(1),
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = TimeSpan.FromMinutes(timerItem1.Minutes),
                Editable = IsMinistryItemEditable(),
                Bell = timerItem1.IsStudentTalk
            });

            startOffset = startOffset.Add(TimeSpan.FromMinutes(timerItem1.Minutes));
            if (timerItem1.IsStudentTalk)
            {
                // counsel...
                startOffset = startOffset.Add(TimeSpan.FromMinutes(1));
            }

            startOffset = startOffset.Add(TimeSpan.FromSeconds(20));

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.MinistryItem2)
            {
                Name = GetMinistryItemTitle(2),
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = TimeSpan.FromMinutes(timerItem2.Minutes),
                Editable = IsMinistryItemEditable(),
                Bell = timerItem2.IsStudentTalk
            });

            startOffset = startOffset.Add(TimeSpan.FromMinutes(timerItem2.Minutes));
            if (timerItem2.IsStudentTalk)
            {
                // counsel...
                startOffset = startOffset.Add(TimeSpan.FromMinutes(1));
            }

            startOffset = startOffset.Add(TimeSpan.FromSeconds(20));

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.MinistryItem3)
            {
                Name = GetMinistryItemTitle(3),
                StartOffsetIntoMeeting = startOffset,
                OriginalDuration = TimeSpan.FromMinutes(timerItem3.Minutes),
                Editable = IsMinistryItemEditable(),
                Bell = timerItem3.IsStudentTalk
            });

            return result;
        }

        private static bool IsPre2018Format()
        {
            return DateTime.Now.Year <= 2017;
        }

        private static bool IsMinistryItemEditable()
        {
            return !IsPre2018Format();
        }

        private static string GetMinistryItemTitle(int item)
        {
            bool oldTitles = IsPre2018Format();

            switch (item)
            {
                case 1:
                    return oldTitles
                        ? Properties.Resources.TALK_INITIAL_CALL
                        : Properties.Resources.MINISTRY1;
                case 2:
                    return oldTitles
                        ? Properties.Resources.TALK_RV
                        : Properties.Resources.MINISTRY2;
                case 3:
                    return oldTitles
                        ? Properties.Resources.TALK_BIBLE_STUDY
                        : Properties.Resources.MINISTRY3;
            }

            throw new ArgumentException(@"Unknown item", nameof(item));
        }

        private static IEnumerable<TalkScheduleItem> GetLivingSchedule(bool isCircuitVisit, Meeting meetingData)
        {
            var result = new List<TalkScheduleItem>();

            TalkTimer timerPart1 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Living1)) ??
                                   new TalkTimer { Minutes = 15, TalkType = TalkTypes.Living1 };

            TalkTimer timerPart2 = meetingData?.Talks.FirstOrDefault(x => x.TalkType.Equals(TalkTypes.Living2));
                                  
            result.Add(new TalkScheduleItem(TalkTypesAutoMode.LivingPart1)
            {
                Name = Properties.Resources.TALK_LIVING1,
                StartOffsetIntoMeeting = new TimeSpan(0, 51, 40),
                OriginalDuration = TimeSpan.FromMinutes(timerPart1.Minutes),
                AllowAdaptive = true,
                Editable = true
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.LivingPart2)
            {
                Name = Properties.Resources.TALK_LIVING2,
                StartOffsetIntoMeeting = new TimeSpan(0, 51, 40).Add(TimeSpan.FromMinutes(timerPart1.Minutes)),
                OriginalDuration = TimeSpan.FromMinutes(timerPart2?.Minutes ?? 0),
                AllowAdaptive = true,
                Editable = true
            });

            if (isCircuitVisit)
            {
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.ConcludingComments)
                {
                    Name = Properties.Resources.TALK_CONCLUDING_COMMENTS,
                    StartOffsetIntoMeeting = new TimeSpan(1, 7, 0),
                    OriginalDuration = TimeSpan.FromMinutes(3),
                    AllowAdaptive = true,
                    Editable = true
                });

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.CircuitServiceTalk)
                {
                    Name = Properties.Resources.TALK_SERVICE,
                    StartOffsetIntoMeeting = new TimeSpan(1, 10, 0),
                    AllowAdaptive = true,
                    OriginalDuration = TimeSpan.FromMinutes(30)
                });
            }
            else
            {
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.CongBibleStudy)
                {
                    Name = Properties.Resources.TALK_CONG_STUDY,
                    StartOffsetIntoMeeting = new TimeSpan(1, 7, 0),
                    OriginalDuration = TimeSpan.FromMinutes(30),
                    AllowAdaptive = true,
                    Editable = true
                });

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.ConcludingComments)
                {
                    Name = Properties.Resources.TALK_CONCLUDING_COMMENTS,
                    StartOffsetIntoMeeting = new TimeSpan(1, 37, 0),
                    OriginalDuration = TimeSpan.FromMinutes(3),
                    AllowAdaptive = true,
                    Editable = true
                });
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

            PrefixDurationsToTalkNames(result);

            return result;
        }

        private static void PrefixDurationsToTalkNames(IReadOnlyCollection<TalkScheduleItem> talks)
        {
            foreach (var talk in talks)
            {
                talk.PrefixDurationToName();
            }
        }

        private static List<TalkScheduleItem> GetWeekendMeetingSchedule(bool isCircuitVisit)
        {
            var result = new List<TalkScheduleItem>();

            if (isCircuitVisit)
            {
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.PublicTalk)
                {
                    Name = Properties.Resources.TALK_PUBLIC,
                    StartOffsetIntoMeeting = new TimeSpan(0, 5, 0),
                    OriginalDuration = TimeSpan.FromMinutes(30),
                    Editable = true
                });

                // song here
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.Watchtower)
                {
                    Name = Properties.Resources.TALK_WT,
                    StartOffsetIntoMeeting = new TimeSpan(0, 40, 0),
                    OriginalDuration = TimeSpan.FromMinutes(30),
                    AllowAdaptive = true,
                    Editable = true
                });

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.CircuitServiceTalk)
                {
                    Name = Properties.Resources.TALK_CONCLUDING,
                    StartOffsetIntoMeeting = new TimeSpan(0, 70, 0),
                    OriginalDuration = TimeSpan.FromMinutes(30),
                    AllowAdaptive = true,
                    Editable = true
                });
            }
            else
            {
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.PublicTalk)
                {
                    Name = Properties.Resources.TALK_PUBLIC,
                    StartOffsetIntoMeeting = new TimeSpan(0, 5, 0),
                    OriginalDuration = TimeSpan.FromMinutes(30),
                    Editable = true
                });

                // song
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.Watchtower)
                {
                    Name = Properties.Resources.TALK_WT,
                    StartOffsetIntoMeeting = new TimeSpan(0, 40, 0),
                    OriginalDuration = TimeSpan.FromMinutes(60),
                    AllowAdaptive = true,
                    Editable = true
                });
            }

            PrefixDurationsToTalkNames(result);

            return result;
        }
    }
}
