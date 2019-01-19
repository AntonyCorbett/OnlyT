namespace OnlyT.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Models;
    using Moq;
    using OnlyT.Services.Options;
    using OnlyT.Services.TalkSchedule;
    using OnlyT.Services.Timer;
    using OnlyT.Tests.Mocks;

    [TestClass]
    public class TestAdaptiveTimer
    {
        private readonly DateTime _theDate = new DateTime(2019, 1, 14);
        private List<TalkScheduleItem> _items;
        private Mock<ITalkScheduleService> _scheduleService;
        private MockDateTimeService _dateTimeService;
        private DateTime _mtgStart;
        private AdaptiveTimerService _adaptiveTimerService;

        [TestInitialize]
        public void InitializeTests()
        {
            _items = GenerateTalkItems(_theDate);

            var options = MockOptions.Create();
            options.GenerateTimingReports = false;
            options.MidWeekAdaptiveMode = AdaptiveMode.TwoWay;
            options.MidWeekOrWeekend = MidWeekOrWeekend.MidWeek;
            options.OperatingMode = OperatingMode.Automatic;

            Mock<IOptionsService> optionsService = new Mock<IOptionsService>();
            optionsService.Setup(o => o.Options).Returns(options);

            _scheduleService = new Mock<ITalkScheduleService>();
            _scheduleService.Setup(x => x.GetTalkScheduleItems()).Returns(_items);
            _scheduleService.Setup(x => x.GetTalkScheduleItem(It.IsAny<int>())).Returns((int id) => _items.Single(y => y.Id == id));

            _dateTimeService = new MockDateTimeService();
            _mtgStart = _theDate + TimeSpan.FromHours(19);  // 7pm
            _dateTimeService.Set(_mtgStart);

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
            TestPerfectSchedule(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestLateSchedule()
        {
            TestLateSchedule(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestEarlySchedule()
        {
            TestEarlySchedule(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestManuallyAdjustedSchedule1()
        {
            TestManuallyAdjustedSchedule1(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        [TestMethod]
        public void TestManuallyAdjustedSchedule2()
        {
            TestManuallyAdjustedSchedule2(_scheduleService, _dateTimeService, _mtgStart, _adaptiveTimerService);
        }

        private void TestPerfectSchedule(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService, 
            DateTime mtgStart, 
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting);

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNull(adaptedDuration1);

            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting);

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNull(adaptedDuration2);

            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            dateTimeService.Set(mtgStart + concluding.StartOffsetIntoMeeting);

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNull(adaptedDuration3);
        }

        private void TestLateSchedule(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            // 10 mins late starting this section.
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting + TimeSpan.FromMinutes(10));

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNotNull(adaptedDuration1);
            AssertTimeSpansAboutEqual(adaptedDuration1.Value, new TimeSpan(0, 11, 52));
            living1.AdaptedDuration = adaptedDuration1;

            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting + TimeSpan.FromMinutes(5));

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNotNull(adaptedDuration2);
            AssertTimeSpansAboutEqual(adaptedDuration2.Value, new TimeSpan(0, 25, 27));
            study.AdaptedDuration = adaptedDuration2;

            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            dateTimeService.Set(mtgStart + concluding.StartOffsetIntoMeeting + TimeSpan.FromMinutes(1));

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNotNull(adaptedDuration3);
            AssertTimeSpansAboutEqual(adaptedDuration3.Value, new TimeSpan(0, 2, 0));
            concluding.AdaptedDuration = adaptedDuration3;
        }

        private void TestEarlySchedule(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            // 10 mins early starting this section.
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            dateTimeService.Set(mtgStart + living1.StartOffsetIntoMeeting - TimeSpan.FromMinutes(10));

            var adaptedDuration1 = service.CalculateAdaptedDuration(living1.Id);
            Assert.IsNotNull(adaptedDuration1);
            AssertTimeSpansAboutEqual(adaptedDuration1.Value, new TimeSpan(0, 18, 7));
            living1.AdaptedDuration = adaptedDuration1;

            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            dateTimeService.Set(mtgStart + study.StartOffsetIntoMeeting - TimeSpan.FromMinutes(5));

            var adaptedDuration2 = service.CalculateAdaptedDuration(study.Id);
            Assert.IsNotNull(adaptedDuration2);
            AssertTimeSpansAboutEqual(adaptedDuration2.Value, new TimeSpan(0, 34, 32));
            study.AdaptedDuration = adaptedDuration2;

            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);
            dateTimeService.Set(mtgStart + concluding.StartOffsetIntoMeeting - TimeSpan.FromMinutes(1));

            var adaptedDuration3 = service.CalculateAdaptedDuration(concluding.Id);
            Assert.IsNotNull(adaptedDuration3);
            AssertTimeSpansAboutEqual(adaptedDuration3.Value, new TimeSpan(0, 4, 0));
            concluding.AdaptedDuration = adaptedDuration3;
        }

        private void TestManuallyAdjustedSchedule1(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);

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

        private void TestManuallyAdjustedSchedule2(
            Mock<ITalkScheduleService> scheduleService,
            MockDateTimeService dateTimeService,
            DateTime mtgStart,
            AdaptiveTimerService service)
        {
            var living1 = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.LivingPart1);
            var study = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.CongBibleStudy);
            var concluding = scheduleService.Object.GetTalkScheduleItem((int)TalkTypesAutoMode.ConcludingComments);

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

        private List<TalkScheduleItem> GenerateTalkItems(DateTime theDate)
        {
            return TalkScheduleAuto.GetMidweekScheduleForTesting(theDate).ToList();
        }

        private void AssertTimeSpansAboutEqual(TimeSpan ts1, TimeSpan ts2)
        {
            Assert.IsTrue(Math.Abs(ts1.TotalSeconds - ts2.TotalSeconds) < 2);
        }
    }
}
