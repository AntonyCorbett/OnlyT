using CommunityToolkit.Mvvm.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlyT.Services.Options;
using OnlyT.Tests.Mocks;
using OnlyT.ViewModel;
using OnlyT.ViewModel.Messages;

namespace OnlyT.Tests
{
    [TestClass]
    public class TestTimerOutputWindowViewModel
    {
        private Options? _options;
        private TimerOutputWindowViewModel? _viewModel;

        [TestInitialize]
        public void InitializeTests()
        {
            _options = MockOptions.Create();
            _options.ShowCountUpTarget = true;

            var optionsService = new Mock<IOptionsService>();
            optionsService.Setup(o => o.Options).Returns(_options);

            var dateTimeService = new MockDateTimeService();
            dateTimeService.Set(new System.DateTime(2020, 1, 6, 19, 0, 0));

            _viewModel = new TimerOutputWindowViewModel(optionsService.Object, dateTimeService);
        }

        [TestCleanup]
        public void CleanupTests()
        {
            WeakReferenceMessenger.Default.UnregisterAll(_viewModel!);
        }

        [TestMethod]
        public void CountUpShowsTargetOnStart()
        {
            WeakReferenceMessenger.Default.Send(new TimerStartMessage(600, 0, true, 1));

            Assert.AreEqual("00:00", _viewModel!.TimeString);
            Assert.AreEqual("10:00", _viewModel.CountUpTargetString);
            Assert.IsTrue(_viewModel.IsCountUpTargetVisible);
        }

        [TestMethod]
        public void CountDownDoesNotShowTarget()
        {
            WeakReferenceMessenger.Default.Send(new TimerStartMessage(600, 0, false, 1));

            Assert.AreEqual("10:00", _viewModel!.TimeString);
            Assert.IsNull(_viewModel.CountUpTargetString);
            Assert.IsFalse(_viewModel.IsCountUpTargetVisible);
        }

        [TestMethod]
        public void CountUpResumeShowsElapsedAndFullTarget()
        {
            // paused after 4 mins of a 10 min talk, so 6 mins remaining...
            WeakReferenceMessenger.Default.Send(new TimerStartMessage(360, 240, true, 1));

            Assert.AreEqual("04:00", _viewModel!.TimeString);
            Assert.AreEqual("10:00", _viewModel.CountUpTargetString);
        }

        [TestMethod]
        public void CountUpResumeAfterDurationEditShowsNewTarget()
        {
            // paused after 4 mins of a 10 min talk, then the remaining time cut to 4 mins...
            WeakReferenceMessenger.Default.Send(new TimerStartMessage(240, 240, true, 1));

            Assert.AreEqual("04:00", _viewModel!.TimeString);
            Assert.AreEqual("08:00", _viewModel.CountUpTargetString);
        }

        [TestMethod]
        public void TargetFollowsOptionChange()
        {
            WeakReferenceMessenger.Default.Send(new TimerStartMessage(600, 0, true, 1));

            _options!.ShowCountUpTarget = false;
            WeakReferenceMessenger.Default.Send(new ShowCountUpTargetChangedMessage());
            Assert.IsNull(_viewModel!.CountUpTargetString);

            _options.ShowCountUpTarget = true;
            WeakReferenceMessenger.Default.Send(new ShowCountUpTargetChangedMessage());
            Assert.AreEqual("10:00", _viewModel.CountUpTargetString);
        }

        [TestMethod]
        public void ClearCountUpTargetHidesTarget()
        {
            WeakReferenceMessenger.Default.Send(new TimerStartMessage(600, 0, true, 1));

            _viewModel!.ClearCountUpTarget();

            Assert.IsNull(_viewModel.CountUpTargetString);
        }
    }
}
