using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlyT.Models;
using OnlyT.Services.Options;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;
using OnlyT.Tests.Mocks;

namespace OnlyT.Tests
{
    [TestClass]
    public class TestAdaptiveTimer
    {
        private readonly DateTime _theDate = new(2020, 1, 6);
        private List<TalkScheduleItem>? _items;
        private Mock<ITalkScheduleService>? _scheduleService;
        private MockDateTimeService? _dateTimeService;
        private DateTime _mtgStart;
        private AdaptiveTimerService? _adaptiveTimerService;

        [TestInitialize]
        public void InitializeTests()
        {
            _dateTimeService = new MockDateTimeService();
            _mtgStart = _theDate + TimeSpan.FromHours(19);  // 7pm
            _dateTimeService.Set(_mtgStart);

            _items = GenerateTalkItems(_theDate);

            var options = MockOptions.Create();
            options.GenerateTimingReports = false;
            options.MidWeekAdaptiveMode = AdaptiveMode.TwoWay;
            options.MidWeekOrWeekend = MidWeekOrWeekend.MidWeek;
            options.OperatingMode = OperatingMode.Automatic;

            var optionsService = new Mock<IOptionsService>();
            optionsService.Setup(o => o.Options).Returns(options);
            optionsService.Setup(x => x.GetAdaptiveMode()).Returns(options.MidWeekAdaptiveMode);

            _scheduleService = new Mock<ITalkScheduleService>();
            _scheduleService.Setup(x => x.GetTalkScheduleItems()).Returns(_items);
            _scheduleService.Setup(x => x.GetTalkScheduleItem(It.IsAny<int>())).Returns((int id) => _items.Single(y => y.Id == id));

            _adaptiveTimerService = new AdaptiveTimerService(
                optionsService.Object, _scheduleService.Object, _dateTimeService);

            _adaptiveTimerService.SetMeetingStartTimeForTesting(_dateTimeService.UtcNow());

            // complete most of the timers...
            foreach (var item in _items)
            {
                if (item.Id == (int)TalkTypesAutoMode.LivingPart1)
                {
                    break;
                }

                // each item exactly on time
                item.CompletedTimeSecs = (int)item.ActualDuration.TotalSeconds;
            }
        }

        [TestMethod]
        public void TestPerfectSchedule()
        {
            Assert.IsNotNull(_scheduleService);
            Assert.IsNotNull(_dateTimeService);
            Assert.IsNotNull(_adaptiveTimerService);

            TestPerfectSchedule(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestLateSchedule()
        {
            Assert.IsNotNull(_scheduleService);
            Assert.IsNotNull(_dateTimeService);
            Assert.IsNotNull(_adaptiveTimerService);

            TestLateSchedule(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestLateSchedule2()
        {
            Assert.IsNotNull(_scheduleService);
            Assert.IsNotNull(_dateTimeService);
            Assert.IsNotNull(_adaptiveTimerService);

            TestLateSchedule2(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestEarlySchedule()
        {
            Assert.IsNotNull(_scheduleService);
            Assert.IsNotNull(_dateTimeService);
            Assert.IsNotNull(_adaptiveTimerService);

            TestEarlySchedule(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestManuallyAdjustedSchedule1()
        {
            Assert.IsNotNull(_scheduleService);
            Assert.IsNotNull(_dateTimeService);
            Assert.IsNotNull(_adaptiveTimerService);

            TestManuallyAdjustedSchedule1(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestManuallyAdjustedSchedule2()
        {
            Assert.IsNotNull(_scheduleService);
            Assert.IsNotNull(_dateTimeService);
            Assert.IsNotNull(_adaptiveTimerService);

            TestManuallyAdjustedSchedule2(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestManuallyAdjustedSchedule3()
        {
            Assert.IsNotNull(_scheduleService);
            Assert.IsNotNull(_dateTimeService);
            Assert.IsNotNull(_adaptiveTimerService);

            TestManuallyAdjustedSchedule3(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestManuallyAdjustedSchedule4()
        {
            Assert.IsNotNull(_scheduleService);
            Assert.IsNotNull(_dateTimeService);
            Assert.IsNotNull(_adaptiveTimerService);

            TestManuallyAdjustedSchedule4(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        private static void TestPerfectSchedule(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService, 
            DateTime mtgStart, 
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            Assert.IsNotNull(living1);
            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting);

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNull(adaptedDuration1);

            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            Assert.IsNotNull(study);
            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting);

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNull(adaptedDuration2);

            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            Assert.IsNotNull(concluding);
            dateTimeService.Set(mtgStart + concluding.StartOffsetIntoMeeting);

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNull(adaptedDuration3);
        }

        private static void TestLateSchedule(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            // 10 mins late starting this section.
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            Assert.IsNotNull(living1);
            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting + TimeSpan.FromMinutes(10));

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNotNull(adaptedDuration1);
            AssertTimeSpansAboutEqual(adaptedDuration1.Value, new TimeSpan(0, 11, 52));
            living1.AdaptedDuration = adaptedDuration1;

            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            Assert.IsNotNull(study);
            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting + TimeSpan.FromMinutes(5));

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNotNull(adaptedDuration2);
            AssertTimeSpansAboutEqual(adaptedDuration2.Value, new TimeSpan(0, 25, 27));
            study.AdaptedDuration = adaptedDuration2;

            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            Assert.IsNotNull(concluding);
            dateTimeService.Set(mtgStart + concluding.StartOffsetIntoMeeting + TimeSpan.FromMinutes(1));

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNotNull(adaptedDuration3);
            AssertTimeSpansAboutEqual(adaptedDuration3.Value, new TimeSpan(0, 2, 0));
            concluding.AdaptedDuration = adaptedDuration3;
        }

        private static void TestLateSchedule2(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            Assert.IsNotNull(living1);
            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            Assert.IsNotNull(concluding);
            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            Assert.IsNotNull(study);

            // 10 mins late starting the "Living" section.
            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting + TimeSpan.FromMinutes(10));

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNotNull(adaptedDuration1);
            AssertTimeSpansAboutEqual(adaptedDuration1.Value, new TimeSpan(0, 11, 52));
            living1.AdaptedDuration = adaptedDuration1;

            // at start of study we're only 5 mins behind
            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting + TimeSpan.FromMinutes(5));

            // remove 5 mins from study and add to conclusion.
            study.ModifiedDuration = study.OriginalDuration.Add(TimeSpan.FromMinutes(-5));
            concluding.ModifiedDuration = concluding.OriginalDuration.Add(TimeSpan.FromMinutes(5));

            // remaining meeting duration = 28 mins shared between study of 25 mins and conclusion of 8 mins (total 33 mins)
            // adapted duration of study = 21 mins 12 secs, conclusion = 6 mins 47 secs

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNotNull(adaptedDuration2);
            AssertTimeSpansAboutEqual(adaptedDuration2.Value, new TimeSpan(0, 21, 12));
            study.AdaptedDuration = adaptedDuration2;

            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting + TimeSpan.FromMinutes(5) + study.AdaptedDuration.Value);
            
            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNotNull(adaptedDuration3);
            AssertTimeSpansAboutEqual(adaptedDuration3.Value, new TimeSpan(0, 6, 47));
            concluding.AdaptedDuration = adaptedDuration3;
        }

        private static void TestEarlySchedule(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            // 10 mins early starting this section.
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            Assert.IsNotNull(living1);
            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting - TimeSpan.FromMinutes(10));

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNotNull(adaptedDuration1);
            AssertTimeSpansAboutEqual(adaptedDuration1.Value, new TimeSpan(0, 18, 7));
            living1.AdaptedDuration = adaptedDuration1;

            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            Assert.IsNotNull(study);
            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting - TimeSpan.FromMinutes(5));

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNotNull(adaptedDuration2);
            AssertTimeSpansAboutEqual(adaptedDuration2.Value, new TimeSpan(0, 34, 32));
            study.AdaptedDuration = adaptedDuration2;

            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            Assert.IsNotNull(concluding);
            dateTimeService.Set(mtgStart + concluding.StartOffsetIntoMeeting - TimeSpan.FromMinutes(1));

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNotNull(adaptedDuration3);
            AssertTimeSpansAboutEqual(adaptedDuration3.Value, new TimeSpan(0, 4, 0));
            concluding.AdaptedDuration = adaptedDuration3;
        }

        private static void TestManuallyAdjustedSchedule1(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            Assert.IsNotNull(living1);
            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            Assert.IsNotNull(study);
            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            Assert.IsNotNull(concluding);

            // add 3 mins to concluding and remove 3 mins from study
            concluding.ModifiedDuration = concluding.OriginalDuration.Add(TimeSpan.FromMinutes(3));
            study.ModifiedDuration = study.OriginalDuration.Add(TimeSpan.FromMinutes(-3));

            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting);

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNull(adaptedDuration1);
            
            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting);

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNull(adaptedDuration2);
            
            dateTimeService.Set(mtgStart + concluding.StartOffsetIntoMeeting - TimeSpan.FromMinutes(3));

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNull(adaptedDuration3);
        }

        private static void TestManuallyAdjustedSchedule2(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            Assert.IsNotNull(living1);
            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            Assert.IsNotNull(study);
            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            Assert.IsNotNull(concluding);

            // add 5 mins to Study and remove 3 mins from Living1 and 2 minutes from concluding
            study.ModifiedDuration = study.OriginalDuration.Add(TimeSpan.FromMinutes(5));
            living1.ModifiedDuration = study.OriginalDuration.Add(-TimeSpan.FromMinutes(3));
            concluding.ModifiedDuration = study.OriginalDuration.Add(-TimeSpan.FromMinutes(2));

            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting);

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNull(adaptedDuration1);

            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting + TimeSpan.FromMinutes(-3));

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNull(adaptedDuration2);

            dateTimeService.Set(mtgStart + concluding.StartOffsetIntoMeeting + TimeSpan.FromMinutes(5));

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNull(adaptedDuration3);
        }

        private static void TestManuallyAdjustedSchedule3(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            Assert.IsNotNull(living1);
            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            Assert.IsNotNull(study);
            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            Assert.IsNotNull(concluding);

            // add 5 mins to concluding but don't compensate elsewhere
            concluding.ModifiedDuration = concluding.OriginalDuration.Add(TimeSpan.FromMinutes(5));
            
            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting);

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNotNull(adaptedDuration1);
            AssertTimeSpansAboutEqual(adaptedDuration1.Value, new TimeSpan(0, 13, 35));
            living1.AdaptedDuration = adaptedDuration1;

            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting + adaptedDuration1.Value);

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNotNull(adaptedDuration2);
            AssertTimeSpansAboutEqual(adaptedDuration2.Value, new TimeSpan(0, 27, 25));
            study.AdaptedDuration = adaptedDuration2;

            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting + adaptedDuration2.Value);

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNotNull(adaptedDuration3);
            AssertTimeSpansAboutEqual(adaptedDuration3.Value, new TimeSpan(0, 5, 34));
            concluding.AdaptedDuration = adaptedDuration3;
        }

        private static void TestManuallyAdjustedSchedule4(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            Assert.IsNotNull(living1);
            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            Assert.IsNotNull(study);
            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            Assert.IsNotNull(concluding);

            // take 5 mins from study but don't compensate elsewhere
            study.ModifiedDuration = study.OriginalDuration.Add(-TimeSpan.FromMinutes(5));

            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting);

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNotNull(adaptedDuration1);
            AssertTimeSpansAboutEqual(adaptedDuration1.Value, new TimeSpan(0, 16, 44));
            living1.AdaptedDuration = adaptedDuration1;

            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting + adaptedDuration1.Value);

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNotNull(adaptedDuration2);
            AssertTimeSpansAboutEqual(adaptedDuration2.Value, new TimeSpan(0, 28, 12));
            study.AdaptedDuration = adaptedDuration2;

            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting + adaptedDuration2.Value);

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNotNull(adaptedDuration3);
            AssertTimeSpansAboutEqual(adaptedDuration3.Value, new TimeSpan(0, 4, 47));
            concluding.AdaptedDuration = adaptedDuration3;
        }

        private List<TalkScheduleItem> GenerateTalkItems(DateTime theDate)
        {
            Assert.IsNotNull(_dateTimeService);

            var isJanuary2020OrLater = _dateTimeService.Now().Date >= new DateTime(2020, 1, 6);
            return TalkScheduleAuto.GetMidweekScheduleForTesting(theDate, isJanuary2020OrLater).ToList();
        }

        private static void AssertTimeSpansAboutEqual(TimeSpan ts1, TimeSpan ts2)
        {
            Assert.IsTrue(Math.Abs(ts1.TotalSeconds - ts2.TotalSeconds) < 2);
        }
    }
}
