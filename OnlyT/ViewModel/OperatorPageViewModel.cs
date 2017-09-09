using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Timer;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
   public class OperatorPageViewModel : ViewModelBase, IPage
   {
      public static string PageName => "OperatorPage";
      private static readonly int _tenMinsInSecs = 600;
      private static readonly string _unknownTalkTitle = "Unknown";
      private readonly ITalkTimerService _timerService;
      private static readonly System.Windows.Media.Brush _whiteBrush = System.Windows.Media.Brushes.White;


      public OperatorPageViewModel(ITalkTimerService timerService)
      {
         _timerService = timerService;
         _timerService.TimerChangedEvent += TimerChangedHandler;

         _targetSeconds= _tenMinsInSecs;
         _talkTitle = _unknownTalkTitle;

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

         TextColor = _whiteBrush;

         RaisePropertyChanged(nameof(IsRunning));
         RaisePropertyChanged(nameof(IsNotRunning));

         StartCommand.RaiseCanExecuteChanged();
         StopCommand.RaiseCanExecuteChanged();
      }

      public bool IsRunning => _timerService.IsRunning;
      public bool IsNotRunning => !IsRunning;

      private void StartTimer()
      {
         RunFlashAnimation = false;
         RunFlashAnimation = true;

         int ms = DateTime.Now.Millisecond;
         if (ms > 100)
         {
            // sync to the second (so that the timer window clock and countdown
            // seconds are in sync)...

            Task.Delay(1000 - ms).Wait();
         }

         _timerService.Start(_targetSeconds);

         RaisePropertyChanged(nameof(IsRunning));
         RaisePropertyChanged(nameof(IsNotRunning));

         StartCommand.RaiseCanExecuteChanged();
         StopCommand.RaiseCanExecuteChanged();
      }

      private bool _runFlashAnimation;
      public bool RunFlashAnimation
      {
         get => _runFlashAnimation;
         set
         {
            if (_runFlashAnimation != value)
            {
               TextColor = new SolidColorBrush(Colors.White);
               _runFlashAnimation = value;
               RaisePropertyChanged(nameof(RunFlashAnimation));
            }
         }
      }

      private System.Windows.Media.Brush _textColor = System.Windows.Media.Brushes.White;
      public System.Windows.Media.Brush TextColor
      {
         get => _textColor;
         set
         {
            _textColor = value;
            RaisePropertyChanged(nameof(TextColor));
         }
      }

      private void TimerChangedHandler(object sender, EventArgs.TimerChangedEventArgs e)
      {
         SecondsRemaining = e.RemainingSecs;
         Messenger.Default.Send(new TimerChangedMessage(e.RemainingSecs));
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

      public string CurrentTimerValueString => TimeFormatter.FormatTimeRemaining(_secondsRemaining);

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
