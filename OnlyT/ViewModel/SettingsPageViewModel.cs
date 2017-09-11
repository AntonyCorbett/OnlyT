using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Models;
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
      private readonly IOptionsService _optionsService;
      private readonly IMonitorsService _monitorsService;

      public SettingsPageViewModel(IMonitorsService monitorsService, IOptionsService optionsService)
      {
         Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
         _optionsService = optionsService;
         _monitorsService = monitorsService;

         _monitors = GetSystemMonitors().ToArray();
         _operatingModes = GetOperatingModes().ToArray();

         NavigateOperatorCommand = new RelayCommand(NavigateOperatorPage);
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
   }
}
