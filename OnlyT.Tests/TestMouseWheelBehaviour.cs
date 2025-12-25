using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlyT.Behaviours;

namespace OnlyT.Tests
{
    [TestClass]
    public class TestMouseWheelBehaviour
    {
        private MouseWheelTimerAdjustBehaviour? _behaviour;
        private Mock<ICommand>? _incrementCommand;
        private Mock<ICommand>? _decrementCommand;
        private Mock<ICommand>? _increment15SecCommand;
        private Mock<ICommand>? _decrement15SecCommand;
        private Mock<ICommand>? _increment5MinCommand;
        private Mock<ICommand>? _decrement5MinCommand;

        [TestInitialize]
        public void Setup()
        {
            _behaviour = new MouseWheelTimerAdjustBehaviour();
            
            _incrementCommand = new Mock<ICommand>();
            _decrementCommand = new Mock<ICommand>();
            _increment15SecCommand = new Mock<ICommand>();
            _decrement15SecCommand = new Mock<ICommand>();
            _increment5MinCommand = new Mock<ICommand>();
            _decrement5MinCommand = new Mock<ICommand>();

            _behaviour.IncrementCommand = _incrementCommand.Object;
            _behaviour.DecrementCommand = _decrementCommand.Object;
            _behaviour.Increment15SecCommand = _increment15SecCommand.Object;
            _behaviour.Decrement15SecCommand = _decrement15SecCommand.Object;
            _behaviour.Increment5MinCommand = _increment5MinCommand.Object;
            _behaviour.Decrement5MinCommand = _decrement5MinCommand.Object;
        }

        [TestMethod]
        public void TestBehaviourDefaultsToDisabled()
        {
            // Arrange & Act
            var behaviour = new MouseWheelTimerAdjustBehaviour();

            // Assert
            Assert.IsFalse(behaviour.IsEnabled, "Behaviour should be disabled by default");
        }

        [TestMethod]
        public void TestCanEnableBehaviour()
        {
            ArgumentNullException.ThrowIfNull(_behaviour);

            // Arrange & Act
            _behaviour.IsEnabled = true;

            // Assert
            Assert.IsTrue(_behaviour.IsEnabled, "Behaviour should be enabled when set to true");
        }

        [TestMethod]
        public void TestCommandPropertiesCanBeSet()
        {
            ArgumentNullException.ThrowIfNull(_behaviour);
            ArgumentNullException.ThrowIfNull(_increment15SecCommand);
            ArgumentNullException.ThrowIfNull(_increment5MinCommand);
            ArgumentNullException.ThrowIfNull(_incrementCommand);
            ArgumentNullException.ThrowIfNull(_decrement15SecCommand);
            ArgumentNullException.ThrowIfNull(_decrement5MinCommand);
            ArgumentNullException.ThrowIfNull(_decrementCommand);

            // Assert
            Assert.AreEqual(_incrementCommand.Object, _behaviour.IncrementCommand);
            Assert.AreEqual(_decrementCommand.Object, _behaviour.DecrementCommand);
            Assert.AreEqual(_increment15SecCommand.Object, _behaviour.Increment15SecCommand);
            Assert.AreEqual(_decrement15SecCommand.Object, _behaviour.Decrement15SecCommand);
            Assert.AreEqual(_increment5MinCommand.Object, _behaviour.Increment5MinCommand);
            Assert.AreEqual(_decrement5MinCommand.Object, _behaviour.Decrement5MinCommand);
        }

        [TestMethod]
        public void TestDependencyPropertyRegistration()
        {
            // Test that all dependency properties are properly registered
            var isEnabledProperty = MouseWheelTimerAdjustBehaviour.IsEnabledProperty;
            var incrementProperty = MouseWheelTimerAdjustBehaviour.IncrementCommandProperty;
            var decrementProperty = MouseWheelTimerAdjustBehaviour.DecrementCommandProperty;
            var increment15Property = MouseWheelTimerAdjustBehaviour.Increment15SecCommandProperty;
            var decrement15Property = MouseWheelTimerAdjustBehaviour.Decrement15SecCommandProperty;
            var increment5Property = MouseWheelTimerAdjustBehaviour.Increment5MinCommandProperty;
            var decrement5Property = MouseWheelTimerAdjustBehaviour.Decrement5MinCommandProperty;

            Assert.IsNotNull(isEnabledProperty);
            Assert.IsNotNull(incrementProperty);
            Assert.IsNotNull(decrementProperty);
            Assert.IsNotNull(increment15Property);
            Assert.IsNotNull(decrement15Property);
            Assert.IsNotNull(increment5Property);
            Assert.IsNotNull(decrement5Property);

            Assert.AreEqual(nameof(MouseWheelTimerAdjustBehaviour.IsEnabled), isEnabledProperty.Name);
            Assert.AreEqual(typeof(bool), isEnabledProperty.PropertyType);
            Assert.AreEqual(typeof(MouseWheelTimerAdjustBehaviour), isEnabledProperty.OwnerType);
        }

        [TestMethod]
        public void TestIsEnabledDefaultValue()
        {
            // Test that the default value for IsEnabled is false
            var defaultValue = MouseWheelTimerAdjustBehaviour.IsEnabledProperty.DefaultMetadata.DefaultValue;
            Assert.AreEqual(false, defaultValue);
        }
    }
}