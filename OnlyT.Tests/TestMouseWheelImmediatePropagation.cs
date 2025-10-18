using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlyT.Services.Options;
using OnlyT.Tests.Mocks;
using OnlyT.ViewModel;
using OnlyT.ViewModel.Messages;
using CommunityToolkit.Mvvm.Messaging;

namespace OnlyT.Tests
{
    [TestClass]
    public class TestMouseWheelImmediatePropagation
    {
        [TestMethod]
        public void TestSettingsPageViewModelSendsMessageWhenMouseWheelSettingChanges()
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

            bool messageReceived = false;
            WeakReferenceMessenger.Default.Register<MouseWheelTimerAdjustChangedMessage>(this, (recipient, message) =>
            {
                messageReceived = true;
            });

            try
            {
                // Act - Change from false to true
                viewModel.AllowMouseWheelTimerAdjust = true;

                // Assert
                Assert.IsTrue(messageReceived, 
                    "MouseWheelTimerAdjustChangedMessage should be sent when setting changes from false to true");

                // Reset and test change from true to false
                messageReceived = false;
                viewModel.AllowMouseWheelTimerAdjust = false;

                Assert.IsTrue(messageReceived, 
                    "MouseWheelTimerAdjustChangedMessage should be sent when setting changes from true to false");
            }
            finally
            {
                WeakReferenceMessenger.Default.Unregister<MouseWheelTimerAdjustChangedMessage>(this);
            }
        }

        [TestMethod]
        public void TestSettingsPageViewModelDoesNotSendMessageWhenValueUnchanged()
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

            bool messageReceived = false;
            WeakReferenceMessenger.Default.Register<MouseWheelTimerAdjustChangedMessage>(this, (recipient, message) =>
            {
                messageReceived = true;
            });

            try
            {
                // Act - Set to same value (default is false)
                viewModel.AllowMouseWheelTimerAdjust = false;

                // Assert
                Assert.IsFalse(messageReceived, 
                    "MouseWheelTimerAdjustChangedMessage should NOT be sent when setting to same value");

                // Test with true value as well
                viewModel.AllowMouseWheelTimerAdjust = true; // This should send message
                messageReceived = false; // Reset
                viewModel.AllowMouseWheelTimerAdjust = true; // This should NOT send message

                Assert.IsFalse(messageReceived, 
                    "MouseWheelTimerAdjustChangedMessage should NOT be sent when setting to same value (true)");
            }
            finally
            {
                WeakReferenceMessenger.Default.Unregister<MouseWheelTimerAdjustChangedMessage>(this);
            }
        }

        [TestMethod]
        public void TestOperatorPageViewModelReceivesMouseWheelMessage()
        {
            // Arrange
            var options = MockOptions.Create();
            var mockOptionsService = new Mock<IOptionsService>();
            mockOptionsService.Setup(x => x.Options).Returns(options);

            var operatorViewModel = new OperatorPageViewModel(
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

            bool propertyChangedFired = false;
            operatorViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(operatorViewModel.AllowMouseWheelTimerAdjust))
                {
                    propertyChangedFired = true;
                }
            };

            // Initial state
            Assert.IsFalse(operatorViewModel.AllowMouseWheelTimerAdjust, 
                "Initial state should be false");

            // Act - Change the underlying option and send message
            options.AllowMouseWheelTimerAdjust = true;
            WeakReferenceMessenger.Default.Send(new MouseWheelTimerAdjustChangedMessage());

            // Assert
            Assert.IsTrue(operatorViewModel.AllowMouseWheelTimerAdjust, 
                "OperatorPageViewModel should reflect the updated option value");
            Assert.IsTrue(propertyChangedFired, 
                "PropertyChanged event should fire when message is received");
        }

        [TestMethod]
        public void TestEndToEndImmediatePropagation()
        {
            // Arrange - Create both ViewModels with shared options
            var options = MockOptions.Create();
            var mockOptionsService = new Mock<IOptionsService>();
            mockOptionsService.Setup(x => x.Options).Returns(options);

            var settingsViewModel = new SettingsPageViewModel(
                Mock.Of<OnlyT.Services.Monitors.IMonitorsService>(),
                Mock.Of<OnlyT.Services.Bell.IBellService>(),
                mockOptionsService.Object,
                Mock.Of<OnlyT.Services.Snackbar.ISnackbarService>(),
                Mock.Of<OnlyT.Services.CountdownTimer.ICountdownTimerTriggerService>(),
                Mock.Of<OnlyT.Common.Services.DateTime.IDateTimeService>(),
                Mock.Of<OnlyT.Services.CommandLine.ICommandLineService>());

            var operatorViewModel = new OperatorPageViewModel(
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

            bool operatorPropertyChangedFired = false;
            operatorViewModel.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(operatorViewModel.AllowMouseWheelTimerAdjust))
                {
                    operatorPropertyChangedFired = true;
                }
            };

            // Initial state verification
            Assert.IsFalse(settingsViewModel.AllowMouseWheelTimerAdjust, 
                "Settings initial state should be false");
            Assert.IsFalse(operatorViewModel.AllowMouseWheelTimerAdjust, 
                "Operator initial state should be false");

            // Act - Change setting in SettingsPageViewModel
            settingsViewModel.AllowMouseWheelTimerAdjust = true;

            // Assert - OperatorPageViewModel should immediately reflect the change
            Assert.IsTrue(settingsViewModel.AllowMouseWheelTimerAdjust, 
                "SettingsPageViewModel should show the new value");
            Assert.IsTrue(operatorViewModel.AllowMouseWheelTimerAdjust, 
                "OperatorPageViewModel should immediately reflect the change");
            Assert.IsTrue(operatorPropertyChangedFired, 
                "OperatorPageViewModel PropertyChanged should fire immediately");

            // Test the reverse change
            operatorPropertyChangedFired = false;
            settingsViewModel.AllowMouseWheelTimerAdjust = false;

            Assert.IsFalse(settingsViewModel.AllowMouseWheelTimerAdjust, 
                "SettingsPageViewModel should show false");
            Assert.IsFalse(operatorViewModel.AllowMouseWheelTimerAdjust, 
                "OperatorPageViewModel should immediately reflect false");
            Assert.IsTrue(operatorPropertyChangedFired, 
                "OperatorPageViewModel PropertyChanged should fire for false change");
        }
    }
}