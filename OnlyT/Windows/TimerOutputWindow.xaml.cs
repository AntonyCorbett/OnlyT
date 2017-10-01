using System.Windows;

namespace OnlyT.Windows
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;

    using GalaSoft.MvvmLight.Messaging;

    using OnlyT.Animations;
    using OnlyT.Services.Options;
    using OnlyT.Utils;
    using OnlyT.ViewModel;
    using OnlyT.ViewModel.Messages;

    /// <summary>
    /// Interaction logic for TimerOutputWindow.xaml
    /// </summary>
    public partial class TimerOutputWindow : Window
    {
        public TimerOutputWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<TimerStartMessage>(this, OnTimerStarted);
            Messenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
        }

        private void OnNavigate(NavigateMessage message)
        {
            if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
            {
                // when the settings page is displayed we ensure that the 
                // display is split so that we can easily adjust the split 
                // position...
                var model = (TimerOutputWindowViewModel)DataContext;
                model.TimeString = TimeFormatter.FormatTimeRemaining(0);
                DisplaySplitScreen();
            }
            else if (message.OriginalPageName.Equals(SettingsPageViewModel.PageName))
            {
                // restore to full screen time of day...
                DisplayFullScreenTimeOfDay();
            }
        }

        private void OnTimerStopped(TimerStopMessage obj)
        {
            DisplayFullScreenTimeOfDay();
        }

        private void DisplayFullScreenTimeOfDay()
        {
            var sb = new Storyboard();

            // fade out timer...
            var fadeOutTimer = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(fadeOutTimer, TimerTextBlock);
            Storyboard.SetTargetProperty(fadeOutTimer, new PropertyPath(OpacityProperty));
            fadeOutTimer.BeginTime = TimeSpan.Zero;

            // fade out clock...
            var fadeOutClock = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(fadeOutClock, ClockPanel);
            Storyboard.SetTargetProperty(fadeOutClock, new PropertyPath(OpacityProperty));
            fadeOutTimer.BeginTime = TimeSpan.Zero;

            GridLengthAnimation rowHeightAdjust1 = null;
            GridLengthAnimation rowHeightAdjust2 = null;
            
            var model = (TimerOutputWindowViewModel)DataContext;
            switch (model.FullScreenClockMode)
            {
                case FullScreenClockMode.Analogue:
                    rowHeightAdjust1 = new GridLengthAnimation();
                    rowHeightAdjust1.From = new GridLength(100, GridUnitType.Star);
                    rowHeightAdjust1.To = new GridLength(100, GridUnitType.Star);
                    rowHeightAdjust1.BeginTime = TimeSpan.FromMilliseconds(500);
                    Storyboard.SetTarget(rowHeightAdjust1, ClockGrid.RowDefinitions[0]);
                    Storyboard.SetTargetProperty(rowHeightAdjust1, new PropertyPath(RowDefinition.HeightProperty));
                    
                    rowHeightAdjust2 = new GridLengthAnimation();
                    rowHeightAdjust2.From = new GridLength(0, GridUnitType.Star);
                    rowHeightAdjust2.To = new GridLength(0, GridUnitType.Star);
                    rowHeightAdjust2.BeginTime = TimeSpan.FromMilliseconds(500);
                    Storyboard.SetTarget(rowHeightAdjust2, ClockGrid.RowDefinitions[1]);
                    Storyboard.SetTargetProperty(rowHeightAdjust2, new PropertyPath(RowDefinition.HeightProperty));
                    break;

                case FullScreenClockMode.Digital:
                    rowHeightAdjust1 = new GridLengthAnimation();
                    rowHeightAdjust1.From = new GridLength(0, GridUnitType.Star);
                    rowHeightAdjust1.To = new GridLength(0, GridUnitType.Star);
                    rowHeightAdjust1.BeginTime = TimeSpan.FromMilliseconds(500);
                    Storyboard.SetTarget(rowHeightAdjust1, ClockGrid.RowDefinitions[0]);
                    Storyboard.SetTargetProperty(rowHeightAdjust1, new PropertyPath(RowDefinition.HeightProperty));

                    rowHeightAdjust2 = new GridLengthAnimation();
                    rowHeightAdjust2.From = new GridLength(100, GridUnitType.Star);
                    rowHeightAdjust2.To = new GridLength(100, GridUnitType.Star);
                    rowHeightAdjust2.BeginTime = TimeSpan.FromMilliseconds(500);
                    Storyboard.SetTarget(rowHeightAdjust2, ClockGrid.RowDefinitions[1]);
                    Storyboard.SetTargetProperty(rowHeightAdjust2, new PropertyPath(RowDefinition.HeightProperty));
                    break;
            }


            // change clock panel to use colspan 2...
            var changeColSpan = new Int32Animation(1, 2, TimeSpan.Zero);
            Storyboard.SetTarget(changeColSpan, ClockPanel);
            Storyboard.SetTargetProperty(changeColSpan, new PropertyPath(Grid.ColumnSpanProperty));
            changeColSpan.BeginTime = TimeSpan.FromMilliseconds(500);

            // fade in the clock panel again...
            var fadeInClock = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(fadeInClock, ClockPanel);
            Storyboard.SetTargetProperty(fadeInClock, new PropertyPath(OpacityProperty));
            fadeInClock.BeginTime = TimeSpan.FromMilliseconds(1000);

            sb.Children.Add(fadeOutTimer);
            sb.Children.Add(fadeOutClock);
            if (rowHeightAdjust1 != null)
            {
                sb.Children.Add(rowHeightAdjust1);
            }
            if (rowHeightAdjust2 != null)
            {
                sb.Children.Add(rowHeightAdjust2);
            }
            sb.Children.Add(changeColSpan);
            sb.Children.Add(fadeInClock);

            sb.Begin();
        }

        private void OnTimerStarted(TimerStartMessage obj)
        {
            DisplaySplitScreen();
        }

        private void DisplaySplitScreen()
        {
            var sb = new Storyboard();

            // fade out clock panel...
            var fadeOutClock = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(fadeOutClock, ClockPanel);
            Storyboard.SetTargetProperty(fadeOutClock, new PropertyPath(OpacityProperty));
            fadeOutClock.BeginTime = TimeSpan.Zero;

            // row heights...
            GridLengthAnimation rowHeightAdjust1 = new GridLengthAnimation();
            rowHeightAdjust1.From = new GridLength(75, GridUnitType.Star);
            rowHeightAdjust1.To = new GridLength(75, GridUnitType.Star);
            rowHeightAdjust1.BeginTime = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTarget(rowHeightAdjust1, ClockGrid.RowDefinitions[0]);
            Storyboard.SetTargetProperty(rowHeightAdjust1, new PropertyPath(RowDefinition.HeightProperty));

            GridLengthAnimation rowHeightAdjust2 = new GridLengthAnimation();
            rowHeightAdjust2.From = new GridLength(25, GridUnitType.Star);
            rowHeightAdjust2.To = new GridLength(25, GridUnitType.Star);
            rowHeightAdjust2.BeginTime = TimeSpan.FromMilliseconds(500);
            Storyboard.SetTarget(rowHeightAdjust2, ClockGrid.RowDefinitions[1]);
            Storyboard.SetTargetProperty(rowHeightAdjust2, new PropertyPath(RowDefinition.HeightProperty));
            
            // restrict clock panel to column 0...
            var changeColSpan = new Int32Animation(2, 1, TimeSpan.Zero);
            Storyboard.SetTarget(changeColSpan, ClockPanel);
            Storyboard.SetTargetProperty(changeColSpan, new PropertyPath(Grid.ColumnSpanProperty));
            changeColSpan.BeginTime = TimeSpan.FromMilliseconds(500);
            
            // fade in the clock panel again...
            var fadeInClock = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(fadeInClock, ClockPanel);
            Storyboard.SetTargetProperty(fadeInClock, new PropertyPath(OpacityProperty));
            fadeInClock.BeginTime = TimeSpan.FromMilliseconds(1000);

            // and fade in the timer...
            var fadeInTimer = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(fadeInTimer, TimerTextBlock);
            Storyboard.SetTargetProperty(fadeInTimer, new PropertyPath(OpacityProperty));
            fadeInTimer.BeginTime = TimeSpan.FromMilliseconds(1000);

            sb.Children.Add(fadeOutClock);
            sb.Children.Add(rowHeightAdjust1);
            sb.Children.Add(rowHeightAdjust2);
            sb.Children.Add(changeColSpan);
            sb.Children.Add(fadeInClock);
            sb.Children.Add(fadeInTimer);
            
            sb.Begin();
        }

        private void InitFullScreenMode()
        {
            var model = (TimerOutputWindowViewModel)DataContext;
            switch (model.FullScreenClockMode)
            {
                case FullScreenClockMode.Analogue:
                    ClockGrid.RowDefinitions[0].Height = new GridLength(100, GridUnitType.Star);
                    ClockGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Star);
                    break;

                case FullScreenClockMode.Digital:
                    ClockGrid.RowDefinitions[0].Height = new GridLength(0, GridUnitType.Star);
                    ClockGrid.RowDefinitions[1].Height = new GridLength(100, GridUnitType.Star);
                    break;

                case FullScreenClockMode.AnalogueAndDigital:
                    ClockGrid.RowDefinitions[0].Height = new GridLength(75, GridUnitType.Star);
                    ClockGrid.RowDefinitions[1].Height = new GridLength(25, GridUnitType.Star);
                    break;
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            InitFullScreenMode();
            WindowState = WindowState.Maximized;
            TheClock.IsRunning = true;
        }
    }
}
