using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.AnalogueClock;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
   internal class TimerOutputWindowViewModel : ViewModelBase
   {
      private static int _secsPerHour = 60 * 60 * 60;

      public TimerOutputWindowViewModel()
      {
         Messenger.Default.Register<TimerChangedMessage>(this, OnTimerChanged);
         Messenger.Default.Register<TimerStartMessage>(this, OnTimerStarted);
         Messenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
      }

      private void OnTimerStopped(TimerStopMessage obj)
      {
         IsRunning = false;
         DurationSector = null;
      }

      private double CalcAngleFromTime(DateTime dt)
      {
         return (dt.Minute + (double)dt.Second / 60) / 60 * 360;
      }

      private void OnTimerStarted(TimerStartMessage message)
      {
         TimeString = TimeFormatter.FormatTimeRemaining(message.TargetSeconds);
         IsRunning = true;
         
         DateTime now = DateTime.Now;
         double startAngle = CalcAngleFromTime(now);

         DateTime endTime = now.AddSeconds(message.TargetSeconds);
         double endAngle = CalcAngleFromTime(endTime);

         if (message.TargetSeconds <= _secsPerHour) // can't display duration sector effectively when > 1 hr
         {
            DurationSector = new DurationSector
            {
               StartAngle = startAngle,
               EndAngle = endAngle,
               CurrentAngle = startAngle,
               IsOvertime = false
            };
         }
      }

      private void OnTimerChanged(TimerChangedMessage message)
      {
         TextColor = GreenYellowRedSelector.GetBrushForTimeRemaining(message.RemainingSecs);
         TimeString = TimeFormatter.FormatTimeRemaining(message.RemainingSecs);

         DateTime now = DateTime.Now;

         if (DurationSector != null)
         {
            var currentAngle = CalcAngleFromTime(now);
            if (Math.Abs(currentAngle - DurationSector.CurrentAngle) > 0.15) // prevent gratuitous updates
            {
               var d = DurationSector.Clone();
               d.CurrentAngle = currentAngle;
               d.IsOvertime = message.RemainingSecs < 0;
               DurationSector = d;
            }
         }
      }

      private string _timeString;
      public string TimeString
      {
         get => _timeString;
         set
         {
            if (_timeString != value)
            {
               _timeString = value;
               RaisePropertyChanged(nameof(TimeString));
            }
         }
      }

      private bool _isRunning;

      public bool IsRunning
      {
         get => _isRunning;
         set
         {
            if (_isRunning != value)
            {
               _isRunning = value;
               RaisePropertyChanged(nameof(IsRunning));
            }
         }
      }
      
      private Brush _textColor = GreenYellowRedSelector.GetGreenBrush();
      public Brush TextColor
      {
         get => _textColor;
         set
         {
            if (!ReferenceEquals(_textColor, value))
            {
               _textColor = value;
               RaisePropertyChanged(nameof(TextColor));
            }
         }
      }

      private DurationSector _durationSector;
      public DurationSector DurationSector
      {
         get => _durationSector;
         set
         {
            if (!ReferenceEquals(_durationSector, value))
            {
               _durationSector = value;
               RaisePropertyChanged(nameof(DurationSector));
            }
         }
      }
   }
}
