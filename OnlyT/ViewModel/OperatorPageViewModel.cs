using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Timer;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
   public class OperatorPageViewModel : ViewModelBase, IPage
   {
      public static string PageName => "OperatorPage";
      private static readonly int _secsPerMinute = 60;
      private static readonly int _tenMinsInSecs = 600;
      private static readonly string _unknownTalkTitle = "Unknown";

      private readonly ITalkTimerService _timerService;


      public OperatorPageViewModel(ITalkTimerService timerService)
      {
         _timerService = timerService;
         _timerService.TimerChangedEvent += TimerChangedHandler;

         TargetSeconds = _tenMinsInSecs;
         TalkTitle = _unknownTalkTitle;

         StartCommand = new RelayCommand(StartTimer, () => IsNotRunning);
         StopCommand = new RelayCommand(StopTimer, () => IsRunning);
         SettingsCommand = new RelayCommand(NavigateSettings, () => IsNotRunning);
      }

      private void NavigateSettings()
      {
         Messenger.Default.Send(new NavigateMessage(SettingsPageViewModel.PageName, null));
      }

      private void StopTimer()
      {
         _timerService.Stop();

         RaisePropertyChanged(nameof(IsRunning));
         RaisePropertyChanged(nameof(IsNotRunning));

         StartCommand.RaiseCanExecuteChanged();
         StopCommand.RaiseCanExecuteChanged();
      }

      public bool IsRunning => _timerService.IsRunning;
      public bool IsNotRunning => !IsRunning;

      private void StartTimer()
      {
         _timerService.Start(_targetSeconds);

         RaisePropertyChanged(nameof(IsRunning));
         RaisePropertyChanged(nameof(IsNotRunning));

         StartCommand.RaiseCanExecuteChanged();
         StopCommand.RaiseCanExecuteChanged();
      }

      private void TimerChangedHandler(object sender, EventArgs.TimerChangedEventArgs e)
      {
         SecondsRemaining = e.RemainingSecs;
      }

      private int _targetSeconds = 0;
      public int TargetSeconds
      {
         get => _targetSeconds;
         set
         {
            if (_targetSeconds != value)
            {
               _targetSeconds = value;
               SecondsRemaining = _targetSeconds;
               RaisePropertyChanged(nameof(TargetSeconds));
               RaisePropertyChanged(nameof(CurrentTimerValueString));
            }
         }
      }

      private int _secondsRemaining = 0;
      public int SecondsRemaining
      {
         get => _secondsRemaining;
         set
         {
            if (_secondsRemaining != value)
            {
               _secondsRemaining = value;
               RaisePropertyChanged(nameof(SecondsRemaining));
               RaisePropertyChanged(nameof(CurrentTimerValueString));
            }
         }
      }

      public string CurrentTimerValueString
      {
         get
         {
            int mins = _secondsRemaining / _secsPerMinute;
            int secs = _secondsRemaining % _secsPerMinute;

            return $"{mins:D2}:{secs:D2}";
         }
      }

      private string _talkTitle;
      public string TalkTitle
      {
         get => _talkTitle;
         set
         {
            if (_talkTitle != value)
            {
               _talkTitle = value;
               RaisePropertyChanged(nameof(TalkTitle));
            }
         }
      }

      public void Activated(object state)
      {
         // todo:

      }

      public RelayCommand StartCommand { get; set; }
      public RelayCommand StopCommand { get; set; }
      public RelayCommand SettingsCommand { get; set; }
   }
}
