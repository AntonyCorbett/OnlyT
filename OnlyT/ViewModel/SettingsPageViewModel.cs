using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Models;
using OnlyT.Services.Monitors;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
   public class SettingsPageViewModel : ViewModelBase, IPage
   {
      public static string PageName => "SettingsPage";
      private readonly MonitorItem[] _monitors;

      public SettingsPageViewModel(IMonitorsService monitorsService)
      {
         _monitors = monitorsService.GetSystemMonitors().ToArray();
         NavigateOperatorCommand = new RelayCommand(NavigateOperatorPage);
      }

      private void NavigateOperatorPage()
      {
         Messenger.Default.Send(new NavigateMessage(OperatorPageViewModel.PageName, null));
      }

      public IEnumerable<MonitorItem> Monitors => _monitors;

      public void Activated(object state)
      {
         
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
