using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace OnlyT.Animations;

/// <inheritdoc />
/// <summary>
/// Animates a GridLength value 
/// </summary>
public class GridLengthAnimation : AnimationTimeline
{
    public static readonly DependencyProperty FromProperty = DependencyProperty.Register(nameof(From), typeof(GridLength), typeof(GridLengthAnimation));
    public static readonly DependencyProperty ToProperty = DependencyProperty.Register(nameof(To), typeof(GridLength), typeof(GridLengthAnimation));

    public override Type TargetPropertyType => typeof(GridLength);

    public GridLength From
    {
        // ReSharper disable once PossibleNullReferenceException
        get => (GridLength)GetValue(FromProperty);
        set => SetValue(FromProperty, value);
    }

    public GridLength To
    {
        // ReSharper disable once PossibleNullReferenceException
        get => (GridLength)GetValue(ToProperty);
        set => SetValue(ToProperty, value);
    }

    public override object GetCurrentValue(
        object defaultOriginValue,
        object defaultDestinationValue, 
        AnimationClock animationClock)
    {
        if(animationClock.CurrentProgress == null)
        {
            return 0;
        }

        // ReSharper disable once PossibleNullReferenceException
        var fromVal = ((GridLength)GetValue(FromProperty)).Value;

        // ReSharper disable once PossibleNullReferenceException
        var toVal = ((GridLength)GetValue(ToProperty)).Value;

        if (fromVal > toVal)
        {
            // ReSharper disable once PossibleInvalidOperationException
            return new GridLength(
                ((1 - animationClock.CurrentProgress.Value) * (fromVal - toVal)) + toVal,
                GridUnitType.Star);
        }

        // ReSharper disable once PossibleInvalidOperationException
        return new GridLength(
            (animationClock.CurrentProgress.Value * (toVal - fromVal)) + fromVal,
            GridUnitType.Star);
    }

    protected override Freezable CreateInstanceCore()
    {
        return new GridLengthAnimation();
    }
}