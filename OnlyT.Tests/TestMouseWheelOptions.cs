using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlyT.Services.Options;
using OnlyT.Tests.Mocks;

namespace OnlyT.Tests
{
    [TestClass]
    public class TestMouseWheelOptions
    {
        [TestMethod]
        public void TestAllowMouseWheelTimerAdjustDefaultValue()
        {
            // Arrange & Act
            var options = new Options();

            // Assert
            Assert.IsFalse(options.AllowMouseWheelTimerAdjust, 
                "AllowMouseWheelTimerAdjust should default to false for safety");
        }

        [TestMethod]
        public void TestAllowMouseWheelTimerAdjustCanBeEnabled()
        {
            // Arrange
            var options = new Options();

            // Act
            options.AllowMouseWheelTimerAdjust = true;

            // Assert
            Assert.IsTrue(options.AllowMouseWheelTimerAdjust, 
                "AllowMouseWheelTimerAdjust should be settable to true");
        }

        [TestMethod]
        public void TestAllowMouseWheelTimerAdjustCanBeDisabled()
        {
            // Arrange
            var options = new Options { AllowMouseWheelTimerAdjust = true };

            // Act
            options.AllowMouseWheelTimerAdjust = false;

            // Assert
            Assert.IsFalse(options.AllowMouseWheelTimerAdjust, 
                "AllowMouseWheelTimerAdjust should be settable to false");
        }

        [TestMethod]
        public void TestMockOptionsIncludesMouseWheelSetting()
        {
            // Arrange & Act
            var mockOptions = MockOptions.Create();

            // Assert - Should have the property accessible without errors
            var initialValue = mockOptions.AllowMouseWheelTimerAdjust;
            mockOptions.AllowMouseWheelTimerAdjust = true;
            var modifiedValue = mockOptions.AllowMouseWheelTimerAdjust;

            Assert.IsFalse(initialValue, "Mock options should start with false");
            Assert.IsTrue(modifiedValue, "Mock options should be modifiable");
        }
    }
}