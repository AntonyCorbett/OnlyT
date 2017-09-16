using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Models;
using OnlyT.Services.Bell;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
   public class SettingsPageViewModel : ViewModelBase, IPage
   {
      public static string PageName => "SettingsPage";
      private readonly MonitorItem[] _monitors;
      private readonly OperatingModeItem[] _operatingModes;
      private readonly AutoMeetingTime[] _autoMeetingTimes;
      private readonly IOptionsService _optionsService;
      private readonly IMonitorsService _monitorsService;
      private readonly IBellService _bellService;

      public SettingsPageViewModel(
         IMonitorsService monitorsService, 
         IBellService bellService,
         IOptionsService optionsService)
      {
         // subscriptions...
         Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
         Messenger.Default.Register<BellStatusChangedMessage>(this, OnBellChanged);

         _optionsService = optionsService;
         _monitorsService = monitorsService;
         _bellService = bellService;

         _monitors = GetSystemMonitors().ToArray();
         _operatingModes = GetOperatingModes().ToArray();
         _autoMeetingTimes = GetAutoMeetingTimes().ToArray();

         NavigateOperatorCommand = new RelayCommand(NavigateOperatorPage);
         TestBellCommand = new RelayCommand(TestBell, IsNotPlayingBell);
      }

      private void OnBellChanged(BellStatusChangedMessage message)
      {
         TestBellCommand.RaiseCanExecuteChanged();
      }

      private bool IsNotPlayingBell()
      {
         return !_bellService.IsPlaying;
      }

      private void TestBell()
      {
         _bellService.Play(_optionsService.Options.BellVolumePercent);
      }

      private IEnumerable<AutoMeetingTime> GetAutoMeetingTimes()
      {
         return new List<AutoMeetingTime>
         {
            new AutoMeetingTime {Name = "Midweek", Id = MidWeekOrWeekend.MidWeek },
            new AutoMeetingTime {Name = "Weekend", Id = MidWeekOrWeekend.Weekend }
         };
      }

      private IEnumerable<OperatingModeItem> GetOperatingModes()
      {
         return new List<OperatingModeItem>
         {
            new OperatingModeItem {Name = "Manual", Mode = OperatingMode.Manual},
            new OperatingModeItem {Name = "File-based", Mode = OperatingMode.ScheduleFile},
            new OperatingModeItem {Name = "Automatic", Mode = OperatingMode.Automatic}
         };
      }

      private IEnumerable<MonitorItem> GetSystemMonitors()
      {
         var result = new List<MonitorItem> {new MonitorItem()};  // empty (i.e. no timer monitor)
         result.AddRange(_monitorsService.GetSystemMonitors());
         return result;
      }

      private void NavigateOperatorPage()
      {
         Save();
         Messenger.Default.Send(new NavigateMessage(OperatorPageViewModel.PageName, null));
      }

      public IEnumerable<MonitorItem> Monitors => _monitors;

      public string MonitorId
      {
         get => _optionsService.Options.TimerMonitorId;
         set
         {
            if (_optionsService.Options.TimerMonitorId != value)
            {
               _optionsService.Options.TimerMonitorId = value;
               RaisePropertyChanged(nameof(MonitorId));
               Messenger.Default.Send(new TimerMonitorChangedMessage());
            }
         }
      }

      public IEnumerable<OperatingModeItem> OperatingModes => _operatingModes;

      public OperatingMode OperatingMode
      {
         get => _optionsService.Options.OperatingMode;
         set
         {
            if (_optionsService.Options.OperatingMode != value)
            {
               _optionsService.Options.OperatingMode = value;
               RaisePropertyChanged(nameof(OperatingMode));
               Messenger.Default.Send(new OperatingModeChangedMessage());
            }
         }
      }

      public IEnumerable<AutoMeetingTime> AutoMeetingTimes => _autoMeetingTimes;

      public MidWeekOrWeekend MidWeekOrWeekend
      {
         get => _optionsService.Options.MidWeekOrWeekend;
         set
         {
            if (_optionsService.Options.MidWeekOrWeekend != value)
            {
               _optionsService.Options.MidWeekOrWeekend = value;
               RaisePropertyChanged(nameof(MidWeekOrWeekend));
               Messenger.Default.Send(new AutoMeetingChangedMessage());
            }
         }
      }

      public bool IsCircuitVisit
      {
         get => _optionsService.Options.IsCircuitVisit;
         set
         {
            if (_optionsService.Options.IsCircuitVisit != value)
            {
               _optionsService.Options.IsCircuitVisit = value;
               RaisePropertyChanged(nameof(IsCircuitVisit));
               Messenger.Default.Send(new AutoMeetingChangedMessage());
            }
         }
      }

      public bool AlwaysOnTop
      {
         get => _optionsService.Options.AlwaysOnTop;
         set
         {
            if (_optionsService.Options.AlwaysOnTop != value)
            {
               _optionsService.Options.AlwaysOnTop = value;
               RaisePropertyChanged(nameof(AlwaysOnTop));
               Messenger.Default.Send(new AlwaysOnTopChangedMessage());
            }
         }
      }

      public bool IsBellEnabled
      {
         get => _optionsService.Options.IsBellEnabled;
         set
         {
            if (_optionsService.Options.IsBellEnabled != value)
            {
               _optionsService.Options.IsBellEnabled = value;
               RaisePropertyChanged(nameof(IsBellEnabled));
            }
         }
      }

      public int BellVolumePercent
      {
         get => _optionsService.Options.BellVolumePercent;
         set
         {
            if (_optionsService.Options.BellVolumePercent != value)
            {
               _optionsService.Options.BellVolumePercent = value;
               RaisePropertyChanged(nameof(BellVolumePercent));
            }
         }
      }

      public void Activated(object state)
      {
         
      }

      private void OnShutDown(ShutDownMessage obj)
      {
         Save();
      }

      private void Save()
      {
         _optionsService.Save();
      }

      public string AppVersionStr => string.Format(Properties.Resources.APP_VER, GetVersionString());

      private string GetVersionString()
      {
         var ver = Assembly.GetExecutingAssembly().GetName().Version;
         return $"{ver.Major}.{ver.Minor}.{ver.Build}.{ver.Revision}";
      }


      public RelayCommand NavigateOperatorCommand { get; set; }
      public RelayCommand TestBellCommand { get; set; }
   }
}
