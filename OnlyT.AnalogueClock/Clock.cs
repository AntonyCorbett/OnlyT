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

      private Line _minuteHand;
      private Line _hourHand;
      private Line _secondHand;
      private readonly DispatcherTimer _timer;
      

      static Clock()
      {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(Clock), 
            new FrameworkPropertyMetadata(typeof(Clock)));
      }

      public Clock()
      {
         _timer = new DispatcherTimer(_timerInterval, DispatcherPriority.Render, 
            TimerCallback, Dispatcher.CurrentDispatcher);
      }

      private void TimerCallback(object sender, EventArgs eventArgs)
      {
         var now = DateTime.Now;

         double secondAngle = now.Second * 6;
         ((DropShadowEffect)_secondHand.Effect).Direction = secondAngle;
         _secondHand.RenderTransform = new RotateTransform(secondAngle, 250, 250);

         double minuteAngle = (now.Minute * 6) + (now.Second + (double)now.Millisecond / 1000) / 60 * 6;
         ((DropShadowEffect)_minuteHand.Effect).Direction = minuteAngle;
         _minuteHand.RenderTransform = new RotateTransform(minuteAngle, 250, 250);

         int hr = now.Hour >= 12? now.Hour-12 : now.Hour;
         double hourAngle = (hr * 30) + ((double)now.Minute / 60) * 30;

         ((DropShadowEffect)_hourHand.Effect).Direction = hourAngle;
         _hourHand.RenderTransform = new RotateTransform(hourAngle, 250, 250);
      }


      public override void OnApplyTemplate()
      {
         base.OnApplyTemplate();

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
