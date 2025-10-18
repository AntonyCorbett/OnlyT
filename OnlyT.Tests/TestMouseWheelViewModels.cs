using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlyT.Services.Options;
using OnlyT.Tests.Mocks;
using OnlyT.ViewModel;

namespace OnlyT.Tests
{
    [TestClass]
    public class TestMouseWheelViewModels
    {
        [TestMethod]
        public void TestSettingsPageViewModelMouseWheelProperty()
        {
            // Arrange
            var options = MockOptions.Create();
            var mockOptionsService = new Mock<IOptionsService>();
            mockOptionsService.Setup(x => x.Options).Returns(options);

            var viewModel = new SettingsPageViewModel(
                Mock.Of<OnlyT.Services.Monitors.IMonitorsService>(),
                Mock.Of<OnlyT.Services.Bell.IBellService>(),
                mockOptionsService.Object,
                Mock.Of<OnlyT.Services.Snackbar.ISnackbarService>(),
                Mock.Of<OnlyT.Services.CountdownTimer.ICountdownTimerTriggerService>(),
                Mock.Of<OnlyT.Common.Services.DateTime.IDateTimeService>(),
                Mock.Of<OnlyT.Services.CommandLine.ICommandLineService>());

            // Test initial value
            Assert.IsFalse(viewModel.AllowMouseWheelTimerAdjust, 
                "SettingsPageViewModel should initially have mouse wheel disabled");

            // Test setting to true
            viewModel.AllowMouseWheelTimerAdjust = true;
            Assert.IsTrue(viewModel.AllowMouseWheelTimerAdjust, 
                "SettingsPageViewModel should reflect true value");
            Assert.IsTrue(options.AllowMouseWheelTimerAdjust, 
                "Underlying options should be updated");

            // Test setting to false
            viewModel.AllowMouseWheelTimerAdjust = false;
            Assert.IsFalse(viewModel.AllowMouseWheelTimerAdjust, 
                "SettingsPageViewModel should reflect false value");
            Assert.IsFalse(options.AllowMouseWheelTimerAdjust, 
                "Underlying options should be updated");
        }

        [TestMethod]
        public void TestOperatorPageViewModelMouseWheelProperty()
        {
            // Arrange
            var options = MockOptions.Create();
            var mockOptionsService = new Mock<IOptionsService>();
            mockOptionsService.Setup(x => x.Options).Returns(options);

            // Create minimal mocks for OperatorPageViewModel dependencies
            var viewModel = new OperatorPageViewModel(
                Mock.Of<OnlyT.Services.Timer.ITalkTimerService>(),
                Mock.Of<OnlyT.Services.TalkSchedule.ITalkScheduleService>(),
                Mock.Of<OnlyT.Services.Timer.IAdaptiveTimerService>(),
                mockOptionsService.Object,
                Mock.Of<OnlyT.Services.CommandLine.ICommandLineService>(),
                Mock.Of<OnlyT.Services.Bell.IBellService>(),
                Mock.Of<OnlyT.Services.Report.ILocalTimingDataStoreService>(),
                Mock.Of<OnlyT.Services.Snackbar.ISnackbarService>(),
                Mock.Of<OnlyT.Common.Services.DateTime.IDateTimeService>(),
                Mock.Of<OnlyT.Common.Services.DateTime.IQueryWeekendService>(),
                Mock.Of<OnlyT.Services.OverrunNotificationService.IOverrunService>());

            // Test initial value (should reflect options)
            Assert.IsFalse(viewModel.AllowMouseWheelTimerAdjust, 
                "OperatorPageViewModel should initially have mouse wheel disabled");

            // Test that it reflects options changes
            options.AllowMouseWheelTimerAdjust = true;
            Assert.IsTrue(viewModel.AllowMouseWheelTimerAdjust, 
                "OperatorPageViewModel should reflect options changes");

            options.AllowMouseWheelTimerAdjust = false;
            Assert.IsFalse(viewModel.AllowMouseWheelTimerAdjust, 
                "OperatorPageViewModel should reflect options changes");
        }

        [TestMethod]
        public void TestSettingsViewModelPropertyChangeNotification()
        {
            // Arrange
            var options = MockOptions.Create();
            var mockOptionsService = new Mock<IOptionsService>();
            mockOptionsService.Setup(x => x.Options).Returns(options);

            var viewModel = new SettingsPageViewModel(
                Mock.Of<OnlyT.Services.Monitors.IMonitorsService>(),
                Mock.Of<OnlyT.Services.Bell.IBellService>(),
                mockOptionsService.Object,
                Mock.Of<OnlyT.Services.Snackbar.ISnackbarService>(),
                Mock.Of<OnlyT.Services.CountdownTimer.ICountdownTimerTriggerService>(),
                Mock.Of<OnlyT.Common.Services.DateTime.IDateTimeService>(),
                Mock.Of<OnlyT.Services.CommandLine.ICommandLineService>());

            bool propertyChangedFired = false;
            viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(viewModel.AllowMouseWheelTimerAdjust))
                {
                    propertyChangedFired = true;
                }
            };

            // Act
            viewModel.AllowMouseWheelTimerAdjust = true;

            // Assert
            Assert.IsTrue(propertyChangedFired, 
                "PropertyChanged event should fire when AllowMouseWheelTimerAdjust changes");
        }

        [TestMethod]
        public void TestSettingsViewModelNoPropertyChangeWhenValueSame()
        {
            // Arrange
            var options = MockOptions.Create();
            var mockOptionsService = new Mock<IOptionsService>();
            mockOptionsService.Setup(x => x.Options).Returns(options);

            var viewModel = new SettingsPageViewModel(
                Mock.Of<OnlyT.Services.Monitors.IMonitorsService>(),
                Mock.Of<OnlyT.Services.Bell.IBellService>(),
                mockOptionsService.Object,
                Mock.Of<OnlyT.Services.Snackbar.ISnackbarService>(),
                Mock.Of<OnlyT.Services.CountdownTimer.ICountdownTimerTriggerService>(),
                Mock.Of<OnlyT.Common.Services.DateTime.IDateTimeService>(),
                Mock.Of<OnlyT.Services.CommandLine.ICommandLineService>());

            bool propertyChangedFired = false;
            viewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(viewModel.AllowMouseWheelTimerAdjust))
                {
                    propertyChangedFired = true;
                }
            };

            // Act - set to same value (false)
            viewModel.AllowMouseWheelTimerAdjust = false;

            // Assert
            Assert.IsFalse(propertyChangedFired, 
                "PropertyChanged event should NOT fire when setting to same value");
        }
    }
}