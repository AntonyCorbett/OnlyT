using System;
using System.Collections.Generic;
using System.Linq;
using OnlyT.MeetingSongsFile;
using OnlyT.Models;
using OnlyT.Services.Options;
using OnlyT.Utils;


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
    /// <summary>
    /// The talk schedule when in "Automatic" operating mode
    /// </summary>
    internal static class TalkScheduleAuto
    {
        /// <summary>
        /// Gets the talk schedule
        /// </summary>
        /// <param name="optionsService"></param>
        /// <returns>A collection of TalkScheduleItem</returns>
        public static IEnumerable<TalkScheduleItem> Read(IOptionsService optionsService)
        {
            bool isCircuitVisit = optionsService.Options.IsCircuitVisit;

            var finder = new MeetingSongsFinder();
            var songsAndTimers = finder.GetSongNumbersAndTimersForToday();

            return optionsService.Options.MidWeekOrWeekend == MidWeekOrWeekend.Weekend
               ? GetWeekendMeetingSchedule(isCircuitVisit)
               : GetMidweekMeetingSchedule(isCircuitVisit, songsAndTimers);
        }

        private static bool IsWeekOfVideoPresentations()
        {
            if (IsPre2018Format())
            {
                // ie according to workbook (where the first week starts on the first Monday of the month)...
                DateTime thisMonday = DateTime.Now.Prev(DayOfWeek.Monday);
                return thisMonday.Day <= 7;
            }

            return false;
        }

        private static List<TalkScheduleItem> GetTreasuresSchedule()
        {
            return new List<TalkScheduleItem>
            {
                new TalkScheduleItem(TalkTypesAutoMode.OpeningComments)
                {
                    Name = Properties.Resources.TALK_OPENING_COMMENTS,
                    StartOffsetIntoMeeting = new TimeSpan(0, 5, 0),
                    Duration = TimeSpan.FromMinutes(3)
                },

                new TalkScheduleItem(TalkTypesAutoMode.TreasuresTalk)
                {   
                    Name = Properties.Resources.TALK_TREASURES,
                    StartOffsetIntoMeeting = new TimeSpan(0, 8, 20),
                    Duration = TimeSpan.FromMinutes(10)
                },

                new TalkScheduleItem(TalkTypesAutoMode.DiggingTalk)
                {
                    Name = Properties.Resources.TALK_DIGGING,
                    StartOffsetIntoMeeting = new TimeSpan(0, 18, 40),
                    Duration = TimeSpan.FromMinutes(8)
                },

                new TalkScheduleItem(TalkTypesAutoMode.Reading)
                {
                    Name = Properties.Resources.TALK_READING,
                    StartOffsetIntoMeeting = new TimeSpan(0, 27, 0),
                    Duration = TimeSpan.FromMinutes(4),
                    Bell = true
                }
            };
        }

        private static IEnumerable<TalkScheduleItem> GetMinistrySchedule(MeetingSongsAndTimers songsAndTimers)
        {
            var result = new List<TalkScheduleItem>();

            TimerValueAndBellFlag timerItem1; 
            TimerValueAndBellFlag timerItem2;
            TimerValueAndBellFlag timerItem3;

            if (IsPre2018Format())
            {
                timerItem1 = new TimerValueAndBellFlag { TimerMinutes = 2, UseBell = true };
                timerItem2 = new TimerValueAndBellFlag { TimerMinutes = 4, UseBell = true };
                timerItem3 = new TimerValueAndBellFlag { TimerMinutes = 6, UseBell = true };
            }
            else
            {
                timerItem1 = songsAndTimers.TimerValues[MeetingSongsAndTimers.MinistryTimer1Key];
                timerItem2 = songsAndTimers.TimerValues[MeetingSongsAndTimers.MinistryTimer2Key];
                timerItem3 = songsAndTimers.TimerValues[MeetingSongsAndTimers.MinistryTimer3Key];
            }

            if (IsWeekOfVideoPresentations())
            {
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.PresentationVideos)
                {
                    Name = Properties.Resources.TALK_PRESENTATIONS,
                    StartOffsetIntoMeeting = new TimeSpan(0, 32, 40),
                    Duration = TimeSpan.FromMinutes(15)
                });
            }
            else
            {
                TimeSpan startOffset = new TimeSpan(0, 32, 20);
                
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.MinistryItem1)
                {
                    Name = GetMinistryItemTitle(1),
                    StartOffsetIntoMeeting = startOffset,
                    Duration = TimeSpan.FromMinutes(timerItem1.TimerMinutes),
                    Editable = IsMinistryItemEditable(),
                    Bell = timerItem1.UseBell
                });

                startOffset = startOffset.Add(TimeSpan.FromMinutes(timerItem1.TimerMinutes));
                if (timerItem1.UseBell)
                {
                    // counsel...
                    startOffset = startOffset.Add(TimeSpan.FromMinutes(1));
                }
                startOffset = startOffset.Add(TimeSpan.FromSeconds(20));

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.MinistryItem2)
                {
                    Name = GetMinistryItemTitle(2),
                    StartOffsetIntoMeeting = startOffset,
                    Duration = TimeSpan.FromMinutes(timerItem2.TimerMinutes),
                    Editable = IsMinistryItemEditable(),
                    Bell = timerItem2.UseBell
                });

                startOffset = startOffset.Add(TimeSpan.FromMinutes(timerItem2.TimerMinutes));
                if (timerItem2.UseBell)
                {
                    // counsel...
                    startOffset = startOffset.Add(TimeSpan.FromMinutes(1));
                }
                startOffset = startOffset.Add(TimeSpan.FromSeconds(20));

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.MinistryItem3)
                {
                    Name = GetMinistryItemTitle(3),
                    StartOffsetIntoMeeting = startOffset,
                    Duration = TimeSpan.FromMinutes(timerItem3.TimerMinutes),
                    Editable = IsMinistryItemEditable(),
                    Bell = timerItem3.UseBell
                });
            }

            return result;
        }

        private static bool IsPre2018Format()
        {
            return DateTime.Now.Year <= 2017;
        }

        private static bool IsMinistryItemEditable()
        {
            return IsPre2018Format();
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

        private static IEnumerable<TalkScheduleItem> GetLivingSchedule(bool isCircuitVisit,
           MeetingSongsAndTimers songsAndTimers)
        {
            var result = new List<TalkScheduleItem>();

            TimerValueAndBellFlag timerPart1 = new TimerValueAndBellFlag {TimerMinutes = 15};
            TimerValueAndBellFlag timerPart2 = new TimerValueAndBellFlag();

            if (songsAndTimers.TimersCount == 2)
            {
                timerPart1 = songsAndTimers.TimerValues[MeetingSongsAndTimers.LivingTimer1Key];
                timerPart2 = songsAndTimers.TimerValues[MeetingSongsAndTimers.LivingTimer2Key];
            }

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.LivingPart1)
            {
                Name = Properties.Resources.TALK_LIVING1,
                StartOffsetIntoMeeting = new TimeSpan(0, 51, 40),
                Duration = TimeSpan.FromMinutes(timerPart1.TimerMinutes),
                AllowAdaptive = true,
                Editable = true
            });

            result.Add(new TalkScheduleItem(TalkTypesAutoMode.LivingPart2)
            {
                Name = Properties.Resources.TALK_LIVING2,
                StartOffsetIntoMeeting = new TimeSpan(0, 51, 40).Add(TimeSpan.FromMinutes(timerPart1.TimerMinutes)),
                Duration = TimeSpan.FromMinutes(timerPart2.TimerMinutes),
                AllowAdaptive = true,
                Editable = true
            });

            if (isCircuitVisit)
            {
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.ConcludingComments)
                {
                    Name = Properties.Resources.TALK_CONCLUDING_COMMENTS,
                    StartOffsetIntoMeeting = new TimeSpan(1, 7, 0),
                    Duration = TimeSpan.FromMinutes(3),
                    AllowAdaptive = true,
                    Editable = true
                });

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.CircuitServiceTalk)
                {
                    Name = Properties.Resources.TALK_SERVICE,
                    StartOffsetIntoMeeting = new TimeSpan(1, 10, 0),
                    AllowAdaptive = true,
                    Duration = TimeSpan.FromMinutes(30)
                });
            }
            else
            {
                result.Add(new TalkScheduleItem(TalkTypesAutoMode.CongBibleStudy)
                {
                    Name = Properties.Resources.TALK_CONG_STUDY,
                    StartOffsetIntoMeeting = new TimeSpan(1, 7, 0),
                    Duration = TimeSpan.FromMinutes(30),
                    AllowAdaptive = true,
                    Editable = true
                });

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.ConcludingComments)
                {
                    Name = Properties.Resources.TALK_CONCLUDING_COMMENTS,
                    StartOffsetIntoMeeting = new TimeSpan(1, 37, 0),
                    Duration = TimeSpan.FromMinutes(3),
                    AllowAdaptive = true,
                    Editable = true
                });
            }

            return result;
        }


        private static List<TalkScheduleItem> GetMidweekMeetingSchedule(bool isCircuitVisit,
           MeetingSongsAndTimers songsAndTimers)
        {
            var result = new List<TalkScheduleItem>();

            // Treasures...
            result.AddRange(GetTreasuresSchedule());

            // Ministry...
            result.AddRange(GetMinistrySchedule(songsAndTimers));

            // Living...
            result.AddRange(GetLivingSchedule(isCircuitVisit, songsAndTimers));

            PrefixDurationsToTalkNames(result);

            return result;
        }

        private static bool ShouldShowHours(IReadOnlyCollection<TalkScheduleItem> talks)
        {
            return talks.Any(x => x.Duration >= TimeSpan.FromHours(1) || x.OriginalDuration >= TimeSpan.FromHours(1));
        }

        private static void PrefixDurationsToTalkNames(IReadOnlyCollection<TalkScheduleItem> talks)
        {
            bool showHours = ShouldShowHours(talks);
            foreach (var talk in talks)
            {
                talk.PrefixDurationToName(showHours);
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
                    Duration = TimeSpan.FromMinutes(30),
                    Editable = true
                });

                // song here

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.Watchtower)
                {
                    Name = Properties.Resources.TALK_WT,
                    StartOffsetIntoMeeting = new TimeSpan(0, 40, 0),
                    Duration = TimeSpan.FromMinutes(30),
                    AllowAdaptive = true,
                    Editable = true
                });

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.CircuitServiceTalk)
                {
                    Name = Properties.Resources.TALK_CONCLUDING,
                    StartOffsetIntoMeeting = new TimeSpan(0, 70, 0),
                    Duration = TimeSpan.FromMinutes(30),
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
                    Duration = TimeSpan.FromMinutes(30),
                    Editable = true
                });

                // song

                result.Add(new TalkScheduleItem(TalkTypesAutoMode.Watchtower)
                {
                    Name = Properties.Resources.TALK_WT,
                    StartOffsetIntoMeeting = new TimeSpan(0, 40, 0),
                    Duration = TimeSpan.FromMinutes(60),
                    AllowAdaptive = true,
                    Editable = true
                });
            }

            return result;
        }
    }
}
