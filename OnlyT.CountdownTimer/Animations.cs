namespace OnlyT.CountdownTimer
{
    using System;
    using System.Windows;
    using System.Windows.Media.Animation;

    public static class Animations
    {
        private const int FadeMs = 1000;
        private const int QuickFadeMs = 600;

        public static void FadeInAndOut(
            FrameworkElement parentContainer, 
            FrameworkElement ctrl, 
            int millisecsToDisplay, 
            EventHandler? onCompleted = null)
        {
            var storyboard = new Storyboard();
            storyboard.Children.Add(CreateFadeAnimation(ctrl, FadeMs, true));

            var fadeOutAnim = CreateFadeAnimation(ctrl, FadeMs, false, onCompleted);
            fadeOutAnim.BeginTime = new TimeSpan(0, 0, 0, 0, millisecsToDisplay);
            storyboard.Children.Add(fadeOutAnim);
            storyboard.Begin(parentContainer);
        }

        public static void DropDown(FrameworkElement parentContainer, FrameworkElement ctrl, int ms, EventHandler? onCompleted = null)
        {
            var storyboard = new Storyboard();
            var duration = TimeSpan.FromMilliseconds(ms);

            var height = ctrl.ActualHeight;

            var animation = new ThicknessAnimation
            {
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.Stop,
                From = new Thickness(ctrl.Margin.Left, ctrl.Margin.Top - height, ctrl.Margin.Right, ctrl.Margin.Bottom),
                To = new Thickness(ctrl.Margin.Left, ctrl.Margin.Top, ctrl.Margin.Right, ctrl.Margin.Bottom)
            };
            
            Storyboard.SetTargetName(animation, ctrl.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));
            storyboard.Children.Add(animation);
            storyboard.Children.Add(CreateFadeAnimation(ctrl, ms, true, onCompleted));

            storyboard.Begin(parentContainer);
        }

        public static Storyboard Flash(FrameworkElement parentContainer, FrameworkElement ctrl, EventHandler? onCompleted = null)
        {
            var anim = CreateFadeAnimation(ctrl, FadeMs, true, onCompleted);
            anim.AutoReverse = true;
            anim.RepeatBehavior = RepeatBehavior.Forever;

            var storyboard = new Storyboard();
            storyboard.Children.Add(anim);
            storyboard.Begin(parentContainer, true);

            return storyboard;
        }

        public static void FlashIn(FrameworkElement parentContainer, FrameworkElement ctrl, EventHandler? onCompleted = null)
        {
            var storyboard = new Storyboard();

            var anim = CreateFadeAnimation(ctrl, 350, false, onCompleted);
            anim.AutoReverse = true;
            anim.RepeatBehavior = new RepeatBehavior(3.0);
            storyboard.Children.Add(anim);
            storyboard.Begin(parentContainer);
        }

        public static void SlideIn(FrameworkElement parentContainer, FrameworkElement ctrl, int ms = 500, EventHandler? onCompleted = null)
        {
            Slide(parentContainer, ctrl, ms, true, onCompleted);
        }

        public static void SlideOut(FrameworkElement parentContainer, FrameworkElement ctrl, int ms = 500, EventHandler? onCompleted = null)
        {
            Slide(parentContainer, ctrl, ms, false, onCompleted);
        }

        public static void FadeIn(FrameworkElement parentContainer, FrameworkElement ctrl, EventHandler? onCompleted = null)
        {
            Fade(parentContainer, ctrl, FadeMs, true, onCompleted);
        }

        public static void FadeIn(FrameworkElement parentContainer, FrameworkElement[] ctrls, EventHandler? onCompleted = null)
        {
            Fade(parentContainer, ctrls, FadeMs, true, onCompleted);
        }

        public static void FadeOut(FrameworkElement parentContainer, FrameworkElement ctrl, EventHandler? onCompleted = null)
        {
            Fade(parentContainer, ctrl, FadeMs, false, onCompleted);
        }

        public static void FadeOut(FrameworkElement parentContainer, FrameworkElement[] ctrls, EventHandler? onCompleted = null)
        {
            Fade(parentContainer, ctrls, FadeMs, false, onCompleted);
        }

        public static void QuickFadeIn(FrameworkElement parentContainer, FrameworkElement ctrl, EventHandler? onCompleted = null)
        {
            Fade(parentContainer, ctrl, QuickFadeMs, true, onCompleted);
        }

        public static void QuickFadeIn(FrameworkElement parentContainer, FrameworkElement[] ctrls, EventHandler? onCompleted = null)
        {
            Fade(parentContainer, ctrls, QuickFadeMs, true, onCompleted);
        }

        public static void QuickFadeOut(FrameworkElement parentContainer, FrameworkElement ctrl, EventHandler? onCompleted = null)
        {
            Fade(parentContainer, ctrl, QuickFadeMs, false, onCompleted);
        }

        public static void QuickFadeOut(FrameworkElement parentContainer, FrameworkElement[] ctrls, EventHandler? onCompleted = null)
        {
            Fade(parentContainer, ctrls, QuickFadeMs, false, onCompleted);
        }

        private static DoubleAnimation CreateFadeAnimation(FrameworkElement ctrl, int ms, bool fadeIn, EventHandler? onCompleted = null)
        {
            var duration = TimeSpan.FromMilliseconds(ms);

            var animation = new DoubleAnimation
            {
                From = fadeIn ? 0.0 : 1.0,
                To = fadeIn ? 1.0 : 0.0,
                Duration = new Duration(duration)
            };

            Storyboard.SetTargetName(animation, ctrl.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));

            if (onCompleted != null)
            {
                animation.Completed += onCompleted;
            }

            return animation;
        }

        private static void Fade(FrameworkElement parentContainer, FrameworkElement ctrl, int ms, bool fadeIn, EventHandler? onCompleted = null)
        {
            var storyboard = new Storyboard();
            storyboard.Children.Add(CreateFadeAnimation(ctrl, ms, fadeIn, onCompleted));
            storyboard.Begin(parentContainer);
        }

        private static void Fade(FrameworkElement parentContainer, FrameworkElement[] ctrls, int ms, bool fadeIn, EventHandler? onCompleted = null)
        {
            var storyboard = new Storyboard();

            foreach (var ctrl in ctrls)
            {
                if (ctrl != null)
                {
                    storyboard.Children.Add(CreateFadeAnimation(ctrl, ms, fadeIn));
                }
            }

            if (onCompleted != null)
            {
                storyboard.Completed += onCompleted;
            }

            storyboard.Begin(parentContainer);
        }

        private static void Slide(FrameworkElement parentContainer, FrameworkElement ctrl, int ms, bool slideIn, EventHandler? onCompleted = null)
        {
            var storyboard = new Storyboard();
            var duration = TimeSpan.FromMilliseconds(ms);

            var height = ctrl.ActualHeight;

            var animation = new ThicknessAnimation
            {
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.Stop
            };

            if (slideIn)
            {
                animation.To = ctrl.Margin;
                animation.From =
                   new Thickness(ctrl.Margin.Left, ctrl.Margin.Top + height, ctrl.Margin.Right, ctrl.Margin.Bottom);
            }
            else
            {
                animation.From = ctrl.Margin;
                animation.To =
                   new Thickness(ctrl.Margin.Left, ctrl.Margin.Top + height, ctrl.Margin.Right, ctrl.Margin.Bottom);
            }

            Storyboard.SetTargetName(animation, ctrl.Name);
            Storyboard.SetTargetProperty(animation, new PropertyPath(FrameworkElement.MarginProperty));
            storyboard.Children.Add(animation);

            storyboard.Children.Add(CreateFadeAnimation(ctrl, ms, slideIn, onCompleted));

            storyboard.Begin(parentContainer);
        }
    }
}
