﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlyT.Tests.Mocks;
using Moq;
using OnlyT.Common.Services.DateTime;
using OnlyT.Services.CommandLine;
using OnlyT.Services.Report;
using OnlyT.Services.Snackbar;
using OnlyT.Services.Bell;
using OnlyT.Services.Options;
using OnlyT.Services.OverrunNotificationService;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;
using OnlyT.Utils;
using OnlyT.ViewModel;

namespace OnlyT.Tests
{
    [TestClass]
    public class TestViewModels
    {
        [TestMethod]
        public void TestOperatorViewStartStop()
        {
            const int talkIdStart = 500;
            const int numTalks = 3;

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
            var scheduleService = new MockTalksScheduleService(talkIdStart, numTalks);
            var bellService = new Mock<IBellService>();
            var commandLineService = new Mock<ICommandLineService>();
            var timingDataService = new Mock<ILocalTimingDataStoreService>();
            var snackbarService = new Mock<ISnackbarService>();
            var dateTimeService = new MockDateTimeService();
            var queryWeekendService = new QueryWeekendService();
            var overrunService = new Mock<IOverrunService>();

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
                queryWeekendService,
                overrunService.Object);

            Assert.IsFalse(vm.IsRunning);
            Assert.IsFalse(vm.IsManualMode);

            for (var n = 0; n < numTalks; ++n)
            {
                var talkId = talkIdStart + n;
                Assert.AreEqual(vm.TalkId, talkId);

                var talk = scheduleService.GetTalkScheduleItem(talkId);
                Assert.IsNotNull(talk);
                Assert.AreEqual(vm.CurrentTimerValueString, TimeFormatter.FormatTimerDisplayString(talk.GetPlannedDurationSeconds()));

                vm.StartCommand.Execute(null);
                Assert.IsTrue(vm.IsRunning);

                Assert.AreEqual(vm.TalkId, talkIdStart + n);

                vm.StopCommand.Execute(null);
                Assert.IsFalse(vm.IsRunning);

                Assert.AreEqual(vm.TalkId, (n == numTalks - 1 ? 0 : talkIdStart + n + 1));
            }
        }
    }
}
