using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Converters;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OnlyT.AnalogueClock
{
   public class Clock : Control
   {
      private static readonly TimeSpan _timerInterval = TimeSpan.FromMilliseconds(100);
      private static readonly TimeSpan _animationTimerInterval = TimeSpan.FromMilliseconds(20);
      private static readonly double _angleTolerance = 0.75;

      private AnglesOfHands _animationTargetAngles;
      private AnglesOfHands _animationCurrentAngles;
      
      private Line _minuteHand;
      private Line _hourHand;
      private Line _secondHand;
      private readonly DispatcherTimer _timer;
      private readonly DispatcherTimer _animationTimer;


      static Clock()
      {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(Clock), 
            new FrameworkPropertyMetadata(typeof(Clock)));
      }

      public Clock()
      {
         _timer = new DispatcherTimer(DispatcherPriority.Render) {Interval = _timerInterval};
         _timer.Tick += TimerCallback;

         _animationTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = _animationTimerInterval };
         _animationTimer.Tick += AnimationCallback;
      }

      private double CalculateAngleSeconds(DateTime dt)
      {
         return dt.Second * 6;
      }

      private double CalculateAngleMinutes(DateTime dt)
      {
         return (dt.Minute * 6) + (dt.Second + (double)dt.Millisecond / 1000) / 60 * 6;
      }

      private double CalculateAngleHours(DateTime dt)
      {
         int hr = dt.Hour >= 12 ? dt.Hour - 12 : dt.Hour;
         return (hr * 30) + ((double)dt.Minute / 60) * 30;
      }

      private void TimerCallback(object sender, EventArgs eventArgs)
      {
         var now = DateTime.Now;

         double secondAngle = CalculateAngleSeconds(now);
         ((DropShadowEffect)_secondHand.Effect).Direction = secondAngle;
         _secondHand.RenderTransform = new RotateTransform(secondAngle, 250, 250);

         double minuteAngle = CalculateAngleMinutes(now);
         ((DropShadowEffect)_minuteHand.Effect).Direction = minuteAngle;
         _minuteHand.RenderTransform = new RotateTransform(minuteAngle, 250, 250);

         double hourAngle = CalculateAngleHours(now);
         ((DropShadowEffect)_hourHand.Effect).Direction = hourAngle;
         _hourHand.RenderTransform = new RotateTransform(hourAngle, 250, 250);
      }

      public static readonly DependencyProperty IsRunningProperty =
         DependencyProperty.Register("IsRunning", typeof(bool), typeof(Clock),
            new FrameworkPropertyMetadata(IsRunningPropertyChanged));

      public bool IsRunning
      {
         // ReSharper disable once PossibleNullReferenceException
         get => (bool) GetValue(IsRunningProperty);
         set => SetValue(IsRunningProperty, value);
      }

      private static void IsRunningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         bool b = (bool) e.NewValue;
         Clock c = (Clock) d;
         if (b)
         {
            c.StartupAnimation();
         }
         else
         {
            c._timer.IsEnabled = false;
         }
      }

      private void StartupAnimation()
      {
         _animationTargetAngles = GenerateTargetAngles();
         _animationCurrentAngles = new AnglesOfHands
         {
            HoursAngle = 0.0,
            MinutesAngle = 0.0,
            SecondsAngle = 0.0
         };
         _animationTimer.Start();
      }

      private AnglesOfHands GenerateTargetAngles()
      {
         DateTime targetTime = DateTime.Now;

         return new AnglesOfHands
         {
            SecondsAngle = CalculateAngleSeconds(targetTime),
            MinutesAngle = CalculateAngleMinutes(targetTime),
            HoursAngle = CalculateAngleHours(targetTime)
         };
      }

      private void AnimationCallback(object sender, EventArgs eventArgs)
      {
         ((DispatcherTimer)sender).Stop();

         _animationCurrentAngles.SecondsAngle = AnimateHand(_secondHand, _animationCurrentAngles.SecondsAngle, _animationTargetAngles.SecondsAngle);
         _animationCurrentAngles.MinutesAngle = AnimateHand(_minuteHand, _animationCurrentAngles.MinutesAngle, _animationTargetAngles.MinutesAngle);
         _animationCurrentAngles.HoursAngle = AnimateHand(_hourHand, _animationCurrentAngles.HoursAngle, _animationTargetAngles.HoursAngle);
         
         if (AnimationShouldContinue())
         {
            ((DispatcherTimer) sender).Start();
         }
         else
         {
            _timer.Start();
         }
      }

      private double AnimateHand(Line hand, double currentAngle, double targetAngle)
      {
         if (Math.Abs(currentAngle - targetAngle) > _angleTolerance)
         {
            double delta = (targetAngle - currentAngle) / 5;
            currentAngle += delta;
            
            ((DropShadowEffect)hand.Effect).Direction = currentAngle;
            hand.RenderTransform = new RotateTransform(currentAngle, 250, 250);
         }

         return currentAngle;
      }

      private bool AnimationShouldContinue()
      {
         return
            Math.Abs(_animationCurrentAngles.SecondsAngle - _animationTargetAngles.SecondsAngle) > _angleTolerance ||
            Math.Abs(_animationCurrentAngles.MinutesAngle - _animationTargetAngles.MinutesAngle) > _angleTolerance ||
            Math.Abs(_animationCurrentAngles.HoursAngle - _animationTargetAngles.HoursAngle) > _angleTolerance;
      }

      public override void OnApplyTemplate()
      {
         base.OnApplyTemplate();

         GetHandRefs();

         if (GetTemplateChild("ClockCanvas") is Canvas cc)
         {
            GenerateHourMarkers(cc);
            GenerateHourNumbers(cc);
         }
      }

      private TextBlock CreateHourNumberTextBlock(int hour)
      {
         return new TextBlock
         {
            Text = hour.ToString(),
            FontSize = 36,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.DarkSlateBlue
         };
      }

      private void GenerateHourNumbers(Canvas canvas)
      {
         double clockRadius = canvas.Width / 2;
         double centrePointRadius = clockRadius - 50;
         double borderSize = 80;

         for (int n=0; n<12; ++n)
         {
            var angle = n * 30;
            var angleRadians = angle * Math.PI / 180;

            var hour = n == 0 ? 12 : n;

            var t = CreateHourNumberTextBlock(hour);
            var b = new Border
            {
               Width = borderSize,
               Height = borderSize,
               Child = t
            };
            
            var x = centrePointRadius * Math.Sin(angleRadians);
            var y = Math.Sqrt(centrePointRadius * centrePointRadius - x * x);

            if (n > 3 && n < 9)
            {
               y = -y;
            }

            canvas.Children.Add(b);
            Canvas.SetLeft(b, clockRadius + x - borderSize / 2);
            Canvas.SetTop(b, clockRadius - y - borderSize / 2);
         }
      }

      private void GenerateHourMarkers(Canvas canvas)
      {
         var angle = 0;
         for (var n = 0; n < 4; ++n)
         {
            var line = CreateMajorHourMarker();
            line.RenderTransform = new RotateTransform(angle += 90, 250, 250);
            canvas.Children.Add(line);
         }

         for (var n = 0; n < 12; ++n)
         {
            if (n % 3 > 0)
            {
               var line = CreateMinorHourMarker();
               line.RenderTransform = new RotateTransform(angle, 250, 250);
               canvas.Children.Add(line);
            }

            angle += 30;
         }

         for (var n = 0; n < 60; ++n)
         {
            if (n % 5 > 0)
            {
               var line = CreateMinuteMarker();
               line.RenderTransform = new RotateTransform(angle, 250, 250);
               canvas.Children.Add(line);
            }

            angle += 6;
         }
      }

      private Line CreateMajorHourMarker()
      {
         return new Line
         {
            Stroke = Brushes.Black,
            StrokeThickness = 7,
            X1 = 250,
            Y1 = 19,
            X2 = 250,
            Y2 = 27
         };
      }

      private Line CreateMinorHourMarker()
      {
         return new Line
         {
            Stroke = Brushes.Black,
            StrokeThickness = 3,
            X1 = 250,
            Y1 = 19,
            X2 = 250,
            Y2 = 25
         };
      }

      private Line CreateMinuteMarker()
      {
         return new Line
         {
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            X1 = 250,
            Y1 = 19,
            X2 = 250,
            Y2 = 25
         };
      }

      private void GetHandRefs()
      {
         if (GetTemplateChild("MinuteHand") is Line line1)
         {
            _minuteHand = line1;
            _minuteHand.Effect = new DropShadowEffect
            {
               Color = Colors.DarkGray,
               BlurRadius = 5,
               ShadowDepth = 3,
               Opacity = 0.4
            };
         }

         if (GetTemplateChild("HourHand") is Line line2)
         {
            _hourHand = line2;
            _hourHand.Effect = new DropShadowEffect
            {
               Color = Colors.DarkGray,
               BlurRadius = 5,
               ShadowDepth = 3,
               Opacity = 1
            };
         }

         if (GetTemplateChild("SecondHand") is Line line3)
         {
            _secondHand = line3;
            _secondHand.Effect = new DropShadowEffect
            {
               Color = Colors.DarkGray,
               BlurRadius = 1.2,
               ShadowDepth = 3,
               Opacity = 0.3
            };
         }
      }
   }
}
