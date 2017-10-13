using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlyT.Services.Options;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;
using OnlyT.Tests.Mocks;
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
            const int TALK_ID_START = 500;
            const int NUM_TALKS = 3;

            Mock<IOptionsService> optionsService = new Mock<IOptionsService>();
            optionsService.Setup(o => o.Options).Returns(MockOptions.Create());

            Mock<ITalkTimerService> timerService = new Mock<ITalkTimerService>();
            Mock<IAdaptiveTimerService> adaptiveTimerService = new Mock<IAdaptiveTimerService>();
            ITalkScheduleService scheduleService = new MockTalksScheduleService(TALK_ID_START, NUM_TALKS);

            OperatorPageViewModel vm = new OperatorPageViewModel(timerService.Object, scheduleService,
               adaptiveTimerService.Object, optionsService.Object);

            Assert.IsFalse(vm.IsRunning);
            Assert.IsFalse(vm.IsManualMode);

            for (int n = 0; n < NUM_TALKS; ++n)
            {
                int talkId = TALK_ID_START + n;
                Assert.IsTrue(vm.TalkId == talkId);

                var talk = scheduleService.GetTalkScheduleItem(talkId);
                Assert.IsNotNull(talk);
                Assert.AreEqual(vm.CurrentTimerValueString, TimeFormatter.FormatTimerDisplayString(talk.GetDurationSeconds()));

                vm.StartCommand.Execute(null);
                Assert.IsTrue(vm.IsRunning);

                Assert.IsTrue(vm.TalkId == TALK_ID_START + n);

                vm.StopCommand.Execute(null);
                Assert.IsFalse(vm.IsRunning);

                Assert.IsTrue(vm.TalkId == (n == NUM_TALKS - 1 ? 0 : TALK_ID_START + n + 1));
            }
        }
    }
}
