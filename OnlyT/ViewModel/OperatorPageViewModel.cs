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
using OnlyT.Models;
using OnlyT.Services.Options;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
   public class OperatorPageViewModel : ViewModelBase, IPage
   {
      public static string PageName => "OperatorPage";
      private static readonly string _unknownTalkTitle = "Unknown";
      private readonly ITalkTimerService _timerService;
      private readonly ITalkScheduleService _scheduleService;
      private readonly IOptionsService _optionsService;
      private static readonly System.Windows.Media.Brush _whiteBrush = System.Windows.Media.Brushes.White;


      public OperatorPageViewModel(
         ITalkTimerService timerService, 
         ITalkScheduleService scheduleService, 
         IOptionsService optionsService)
      {
         _scheduleService = scheduleService;
         _optionsService = optionsService;
         _timerService = timerService;
         _timerService.TimerChangedEvent += TimerChangedHandler;

         SelectFirstTalk();

         _talkTitle = _unknownTalkTitle;

         StartCommand = new RelayCommand(StartTimer, () => IsNotRunning);
         StopCommand = new RelayCommand(StopTimer, () => IsRunning);
         SettingsCommand = new RelayCommand(NavigateSettings, () => IsNotRunning);

         // subscriptions...
         Messenger.Default.Register<OperatingModeChangedMessage>(this, OnOperatingModeChanged);
      }

      private void OnOperatingModeChanged(OperatingModeChangedMessage message)
      {
         RaisePropertyChanged(nameof(IsManualMode));
         RaisePropertyChanged(nameof(IsNotManualMode));
      }

      private void SelectFirstTalk()
      {
         var talks = _scheduleService.GetTalkScheduleItems();
         if (talks != null && talks.Any())
         {
            TalkId = talks.First().Id;
         }
      }

      private void NavigateSettings()
      {
         Messenger.Default.Send(new NavigateMessage(SettingsPageViewModel.PageName, null));
      }

      private void StopTimer()
      {
         _timerService.Stop();
         _isStarting = false;
         
         TextColor = _whiteBrush;

         RaisePropertyChanged(nameof(IsRunning));
         RaisePropertyChanged(nameof(IsNotRunning));

         Messenger.Default.Send(new TimerStopMessage());
         RaiseCanExecuteChanged();
      }

      public bool IsManualMode => _optionsService.Options.OperatingMode == OperatingMode.Manual;
      public bool IsNotManualMode => _optionsService.Options.OperatingMode != OperatingMode.Manual;


      public bool IsRunning => _timerService.IsRunning || _isStarting;
      public bool IsNotRunning => !IsRunning;

      private bool _isStarting;

      private void StartTimer()
      {
         _isStarting = true;

         RunFlashAnimation = false;
         RunFlashAnimation = true;

         RaisePropertyChanged(nameof(IsRunning));
         RaisePropertyChanged(nameof(IsNotRunning));

         RaiseCanExecuteChanged();

         Messenger.Default.Send(new TimerStartMessage(_targetSeconds));

         Task.Run(() =>
         {
            int ms = DateTime.Now.Millisecond;
            if (ms > 100)
            {
               // sync to the second (so that the timer window clock and countdown
               // seconds are in sync)...

               Task.Delay(1000 - ms).Wait();
            }

            if (_isStarting)
            {
               _timerService.Start(_targetSeconds);
            }
         });
      }

      private void RaiseCanExecuteChanged()
      {
         StartCommand.RaiseCanExecuteChanged();
         StopCommand.RaiseCanExecuteChanged();
         SettingsCommand.RaiseCanExecuteChanged();
      }

      public IEnumerable<TalkScheduleItem> Talks => _scheduleService.GetTalkScheduleItems();

      private int _talkId;
      public int TalkId
      {
         get => _talkId;
         set
         {
            if (_talkId != value)
            {
               _talkId = value;
               TargetSeconds = GetTargetSecondsFromTalkSchedule(_talkId);
               RaisePropertyChanged(nameof(TalkId));
            }
         }
      }

      private int GetTargetSecondsFromTalkSchedule(int talkId)
      {
         var talk = _scheduleService.GetTalkScheduleItem(talkId);
         return talk?.GetDurationSeconds() ?? 0;
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
         TextColor = GreenYellowRedSelector.GetBrushForTimeRemaining(e.RemainingSecs);
         SecondsRemaining = e.RemainingSecs;
         Messenger.Default.Send(new TimerChangedMessage(e.RemainingSecs));
      }

      private int _targetSeconds;
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

      private int _secondsRemaining;
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

      }

      public RelayCommand StartCommand { get; set; }
      public RelayCommand StopCommand { get; set; }
      public RelayCommand SettingsCommand { get; set; }
   }
}
