namespace OnlyT.Windows
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using Animations;
    using GalaSoft.MvvmLight.Messaging;
    using OnlyT.AnalogueClock;
    using OnlyT.Common.Services.DateTime;
    using Services.Options;
    using Utils;
    using ViewModel;
    using ViewModel.Messages;

    /// <summary>
    /// Interaction logic for TimerOutputWindow.xaml
    /// </summary>
    public partial class TimerOutputWindow : Window
    {
        private readonly DispatcherTimer _persistTimer = new DispatcherTimer(DispatcherPriority.ApplicationIdle);
        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;
        private bool _persistingTalkDuration;
        
        public TimerOutputWindow(
            IOptionsService optionsService, 
            IDateTimeService dateTimeService)
        {
            InitializeComponent();

            _optionsService = optionsService;
            _dateTimeService = dateTimeService;

            Messenger.Default.Register<TimerStartMessage>(this, OnTimerStarted);
            Messenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
            
            _persistTimer.Tick += HandlePersistTimerTick;
        }

        private void HandlePersistTimerTick(object sender, EventArgs e)
        {
            if (_persistingTalkDuration)
            {
                _persistingTalkDuration = false;
                AdjustDisplayOnTimerStop();
            }
        }

        private void OnNavigate(NavigateMessage message)
        {
            if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
            {
                // when the settings page is displayed we ensure that the 
                // display is split so that we can easily adjust the split 
                // position...
                var model = (TimerOutputWindowViewModel)DataContext;
                model.TimeString = TimeFormatter.FormatTimerDisplayString(0);
                DisplaySplitScreen();
            }
            else if (message.OriginalPageName.Equals(SettingsPageViewModel.PageName))
            {
                // restore to full screen time of day...
                DisplayFullScreenTimeOfDay();
            }
        }

        private void OnTimerStopped(TimerStopMessage msg)
        {
            if (msg.PersistFinalTimerValue)
            {
                _persistingTalkDuration = true;
                _persistTimer.Interval = TimeSpan.FromSeconds(_optionsService.Options.PersistDurationSecs);
                _persistTimer.Start();
            }
            else
            {
                AdjustDisplayOnTimerStop();
            }
        }

        private void AdjustDisplayOnTimerStop()
        {
            var model = (TimerOutputWindowViewModel)DataContext;
            if (!model.SplitAndFullScreenModeIdentical())
            {
                // only animate if the user has configured different display
                // layout for timer mode and full-screen mode
                DisplayFullScreenTimeOfDay();
            }
        }

        private void DisplayFullScreenTimeOfDay()
        {
            var sb = new Storyboard();

            // fade out timer...
            var fadeOutTimer = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(fadeOutTimer, TimerPanel);
            Storyboard.SetTargetProperty(fadeOutTimer, new PropertyPath(OpacityProperty));
            fadeOutTimer.BeginTime = TimeSpan.Zero;

            // fade out clock...
            var fadeOutClock = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(400));
            Storyboard.SetTarget(fadeOutClock, ClockPanel);
            Storyboard.SetTargetProperty(fadeOutClock, new PropertyPath(OpacityProperty));
            fadeOutClock.BeginTime = TimeSpan.Zero;

            GridLengthAnimation rowHeightAdjust1 = null;
            GridLengthAnimation rowHeightAdjust2 = null;

            var model = (TimerOutputWindowViewModel)DataContext;
            switch (model.FullScreenClockMode)
            {
                case FullScreenClockMode.Analogue:
                    rowHeightAdjust1 = new GridLengthAnimation
                    {
                        From = new GridLength(100, GridUnitType.Star),
                        To = new GridLength(100, GridUnitType.Star),
                        BeginTime = TimeSpan.FromMilliseconds(500)
                    };
                    Storyboard.SetTarget(rowHeightAdjust1, ClockGrid.RowDefinitions[0]);
                    Storyboard.SetTargetProperty(rowHeightAdjust1, new PropertyPath(RowDefinition.HeightProperty));

                    rowHeightAdjust2 = new GridLengthAnimation
                    {
                        From = new GridLength(0, GridUnitType.Star),
                        To = new GridLength(0, GridUnitType.Star),
                        BeginTime = TimeSpan.FromMilliseconds(500)
                    };
                    Storyboard.SetTarget(rowHeightAdjust2, ClockGrid.RowDefinitions[1]);
                    Storyboard.SetTargetProperty(rowHeightAdjust2, new PropertyPath(RowDefinition.HeightProperty));
                    break;

                case FullScreenClockMode.Digital:
                    rowHeightAdjust1 = new GridLengthAnimation
                    {
                        From = new GridLength(0, GridUnitType.Star),
                        To = new GridLength(0, GridUnitType.Star),
                        BeginTime = TimeSpan.FromMilliseconds(500)
                    };
                    Storyboard.SetTarget(rowHeightAdjust1, ClockGrid.RowDefinitions[0]);
                    Storyboard.SetTargetProperty(rowHeightAdjust1, new PropertyPath(RowDefinition.HeightProperty));

                    rowHeightAdjust2 = new GridLengthAnimation
                    {
                        From = new GridLength(100, GridUnitType.Star),
                        To = new GridLength(100, GridUnitType.Star),
                        BeginTime = TimeSpan.FromMilliseconds(500)
                    };
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

        private void OnTimerStarted(TimerStartMessage msg)
        {
            var model = (TimerOutputWindowViewModel)DataContext;
            if (!model.SplitAndFullScreenModeIdentical() && !_persistingTalkDuration)
            {
                model.TextColor = GreenYellowRedSelector.GetGreenBrush();
                
                // only animate if the user has configured different display
                // layout for timer mode and full-screen mode
                DisplaySplitScreen();
            }

            _persistingTalkDuration = false;
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
            GridLengthAnimation rowHeightAdjust1 = new GridLengthAnimation
            {
                From = new GridLength(75, GridUnitType.Star),
                To = new GridLength(75, GridUnitType.Star),
                BeginTime = TimeSpan.FromMilliseconds(500)
            };

            Storyboard.SetTarget(rowHeightAdjust1, ClockGrid.RowDefinitions[0]);
            Storyboard.SetTargetProperty(rowHeightAdjust1, new PropertyPath(RowDefinition.HeightProperty));

            GridLengthAnimation rowHeightAdjust2 = new GridLengthAnimation
            {
                From = new GridLength(25, GridUnitType.Star),
                To = new GridLength(25, GridUnitType.Star),
                BeginTime = TimeSpan.FromMilliseconds(500)
            };

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
            Storyboard.SetTarget(fadeInTimer, TimerPanel);
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

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var model = (TimerOutputWindowViewModel)DataContext;
            if (!model.ApplicationClosing)
            {
                // prevent window from being closed independently of application.
                e.Cancel = true;
            }
        }

        private void ClockQueryDateTimeEvent(object sender, DateTimeQueryEventArgs e)
        {
            e.DateTime = _dateTimeService.Now();
        }
    }
}
