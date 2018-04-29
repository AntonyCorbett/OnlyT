namespace OnlyT.Animations
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;

    /// <inheritdoc />
    /// <summary>
    /// Animates a GridLength value 
    /// </summary>
    public class GridLengthAnimation : AnimationTimeline
    {
        static GridLengthAnimation()
        {
            FromProperty = DependencyProperty.Register("From", typeof(GridLength), typeof(GridLengthAnimation));
            ToProperty = DependencyProperty.Register("To", typeof(GridLength), typeof(GridLengthAnimation));
        }

        public override Type TargetPropertyType => typeof(GridLength);

        public static readonly DependencyProperty FromProperty;

        public GridLength From
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (GridLength)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty ToProperty;

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
}
