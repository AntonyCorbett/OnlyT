using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
   internal class TimerOutputWindowViewModel : ViewModelBase
   {
      public TimerOutputWindowViewModel()
      {
         _timeString = "10:00";
         Messenger.Default.Register<TimerChangedMessage>(this, OnTimerChanged);
      }

      private void OnTimerChanged(TimerChangedMessage message)
      {
         TimeString = TimeFormatter.FormatTimeRemaining(message.RemainingSecs);
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

      public override void Cleanup()
      {
         // todo: amy cleanup here
         base.Cleanup();
      }
   }
}
