using System;

namespace OnlyT.Tests
{
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

            Mock<ITalkTimerService> timerService = new Mock<ITalkTimerService>();
            Mock<IAdaptiveTimerService> adaptiveTimerService = new Mock<IAdaptiveTimerService>();
            ITalkScheduleService scheduleService = new MockTalksScheduleService(TalkIdStart, NumTalks);
            Mock<IBellService> bellService = new Mock<IBellService>();
            Mock<ICommandLineService> commandLineService = new Mock<ICommandLineService>();
            Mock<ILocalTimingDataStoreService> timingDataService = new Mock<ILocalTimingDataStoreService>();
            Mock<ISnackbarService> snackbarService = new Mock<ISnackbarService>();
            MockDateTimeService dateTimeService = new MockDateTimeService();
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

            for (int n = 0; n < NumTalks; ++n)
            {
                int talkId = TalkIdStart + n;
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
