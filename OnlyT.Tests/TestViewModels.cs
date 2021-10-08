namespace OnlyT.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mocks;
    using Moq;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.Services.CommandLine;
    using OnlyT.Services.Report;
    using OnlyT.Services.Snackbar;
    using Services.Bell;
    using Services.Options;
    using Services.TalkSchedule;
    using Services.Timer;
    using Utils;
    using ViewModel;

    [TestClass]
    public class TestViewModels
    {
        [TestMethod]
        public void TestOperatorViewStartStop()
        {
            const int TalkIdStart = 500;
            const int NumTalks = 3;

            var options = MockOptions.Create();
            options.GenerateTimingReports = false;
            options.MidWeekAdaptiveMode = AdaptiveMode.TwoWay;
            options.MidWeekOrWeekend = MidWeekOrWeekend.MidWeek;
            options.OperatingMode = OperatingMode.Automatic;

            var optionsService = new Mock<IOptionsService>();
            optionsService.Setup(o => o.Options).Returns(options);
            optionsService.Setup(x => x.GetAdaptiveMode()).Returns(options.MidWeekAdaptiveMode);

            var timerService = new Mock<ITalkTimerService>();
            var adaptiveTimerService = new Mock<IAdaptiveTimerService>();
            ITalkScheduleService scheduleService = new MockTalksScheduleService(TalkIdStart, NumTalks);
            var bellService = new Mock<IBellService>();
            var commandLineService = new Mock<ICommandLineService>();
            var timingDataService = new Mock<ILocalTimingDataStoreService>();
            var snackbarService = new Mock<ISnackbarService>();
            var dateTimeService = new MockDateTimeService();
            IQueryWeekendService queryWeekendService = new QueryWeekendService();
            
            dateTimeService.Set(new DateTime(2019, 11, 28) + TimeSpan.FromHours(19));

            var vm = new OperatorPageViewModel(
                timerService.Object, 
                scheduleService,
                adaptiveTimerService.Object, 
                optionsService.Object,
                commandLineService.Object,
                bellService.Object,
                timingDataService.Object,
                snackbarService.Object,
                dateTimeService,
                queryWeekendService);

            Assert.IsFalse(vm.IsRunning);
            Assert.IsFalse(vm.IsManualMode);

            for (var n = 0; n < NumTalks; ++n)
            {
                var talkId = TalkIdStart + n;
                Assert.IsTrue(vm.TalkId == talkId);

                var talk = scheduleService.GetTalkScheduleItem(talkId);
                Assert.IsNotNull(talk);
                Assert.AreEqual(vm.CurrentTimerValueString, TimeFormatter.FormatTimerDisplayString(talk.GetPlannedDurationSeconds()));

                vm.StartCommand.Execute(null);
                Assert.IsTrue(vm.IsRunning);

                Assert.IsTrue(vm.TalkId == TalkIdStart + n);

                vm.StopCommand.Execute(null);
                Assert.IsFalse(vm.IsRunning);

                Assert.IsTrue(vm.TalkId == (n == NumTalks - 1 ? 0 : TalkIdStart + n + 1));
            }
        }
    }
}
