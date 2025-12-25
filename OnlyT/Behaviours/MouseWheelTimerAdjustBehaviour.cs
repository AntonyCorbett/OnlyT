using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace OnlyT.Behaviours;

/// <summary>
/// Behavior that allows adjusting timer value using the mouse wheel when enabled
/// </summary>
public class MouseWheelTimerAdjustBehaviour : Behavior<FrameworkElement>
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.Register(
            nameof(IsEnabled),
            typeof(bool),
            typeof(MouseWheelTimerAdjustBehaviour),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IncrementCommandProperty =
        DependencyProperty.Register(
            nameof(IncrementCommand),
            typeof(ICommand),
            typeof(MouseWheelTimerAdjustBehaviour));

    public static readonly DependencyProperty DecrementCommandProperty =
        DependencyProperty.Register(
            nameof(DecrementCommand),
            typeof(ICommand),
            typeof(MouseWheelTimerAdjustBehaviour));

    public static readonly DependencyProperty Increment15SecCommandProperty =
        DependencyProperty.Register(
            nameof(Increment15SecCommand),
            typeof(ICommand),
            typeof(MouseWheelTimerAdjustBehaviour));

    public static readonly DependencyProperty Decrement15SecCommandProperty =
        DependencyProperty.Register(
            nameof(Decrement15SecCommand),
            typeof(ICommand),
            typeof(MouseWheelTimerAdjustBehaviour));

    public static readonly DependencyProperty Increment5MinCommandProperty =
        DependencyProperty.Register(
            nameof(Increment5MinCommand),
            typeof(ICommand),
            typeof(MouseWheelTimerAdjustBehaviour));

    public static readonly DependencyProperty Decrement5MinCommandProperty =
        DependencyProperty.Register(
            nameof(Decrement5MinCommand),
            typeof(ICommand),
            typeof(MouseWheelTimerAdjustBehaviour));

    /// <summary>
    /// Command to increment timer by 1 minute (default scroll)
    /// </summary>
    public ICommand? IncrementCommand
    {
        get => (ICommand?)GetValue(IncrementCommandProperty);
        set => SetValue(IncrementCommandProperty, value);
    }

    /// <summary>
    /// Command to decrement timer by 1 minute (default scroll)
    /// </summary>
    public ICommand? DecrementCommand
    {
        get => (ICommand?)GetValue(DecrementCommandProperty);
        set => SetValue(DecrementCommandProperty, value);
    }

    /// <summary>
    /// Command to increment timer by 15 seconds (Ctrl + scroll)
    /// </summary>
    public ICommand? Increment15SecCommand
    {
        get => (ICommand?)GetValue(Increment15SecCommandProperty);
        set => SetValue(Increment15SecCommandProperty, value);
    }

    /// <summary>
    /// Command to decrement timer by 15 seconds (Ctrl + scroll)
    /// </summary>
    public ICommand? Decrement15SecCommand
    {
        get => (ICommand?)GetValue(Decrement15SecCommandProperty);
        set => SetValue(Decrement15SecCommandProperty, value);
    }

    /// <summary>
    /// Command to increment timer by 5 minutes (Shift + scroll)
    /// </summary>
    public ICommand? Increment5MinCommand
    {
        get => (ICommand?)GetValue(Increment5MinCommandProperty);
        set => SetValue(Increment5MinCommandProperty, value);
    }

    /// <summary>
    /// Command to decrement timer by 5 minutes (Shift + scroll)
    /// </summary>
    public ICommand? Decrement5MinCommand
    {
        get => (ICommand?)GetValue(Decrement5MinCommandProperty);
        set => SetValue(Decrement5MinCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets whether mouse wheel timer adjustment is enabled
    /// </summary>
    public bool IsEnabled
    {
        get => (bool)GetValue(IsEnabledProperty);
        set => SetValue(IsEnabledProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseWheel += OnMouseWheel;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseWheel -= OnMouseWheel;
        base.OnDetaching();
    }

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Check if the feature is enabled
        if (!IsEnabled)
        {
            return;
        }

        var isScrollUp = e.Delta > 0;
        var isCtrlPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
        var isShiftPressed = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

        ICommand? commandToExecute = null;

        if (isShiftPressed)
        {
            // Shift + scroll: 5 minute increments
            commandToExecute = isScrollUp ? Increment5MinCommand : Decrement5MinCommand;
        }
        else if (isCtrlPressed)
        {
            // Ctrl + scroll: 15 second increments
            commandToExecute = isScrollUp ? Increment15SecCommand : Decrement15SecCommand;
        }
        else
        {
            // Default scroll: 1 minute increments
            commandToExecute = isScrollUp ? IncrementCommand : DecrementCommand;
        }

        if (commandToExecute?.CanExecute(null) == true)
        {
            commandToExecute.Execute(null);
            e.Handled = true;
        }
    }
}