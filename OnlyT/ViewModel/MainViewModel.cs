using System.Windows.Documents;
using GalaSoft.MvvmLight;
using System.Windows;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.ViewModel.Messages;
using System;
using OnlyT.Windows;
using System.ComponentModel;
using OnlyT.Timer;
using System.Threading.Tasks;
using System.Windows.Media;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;

namespace OnlyT.ViewModel
{
   public class MainViewModel : ViewModelBase
   {
      private readonly Dictionary<string, FrameworkElement> _pages = new Dictionary<string, FrameworkElement>();
      private TimerOutputWindow _timerWindow;
      private readonly IOptionsService _optionsService;
      private readonly IMonitorsService _monitorsService;
      private string _currentPageName;


      public MainViewModel(
         ITalkTimerService timerService, 
         IOptionsService optionsService, 
         IMonitorsService monitorsService)
      {
         _optionsService = optionsService;
         _monitorsService = monitorsService;

         Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
         Messenger.Default.Register<TimerMonitorChangedMessage>(this, OnTimerMonitorChanged);
         
         _pages.Add(OperatorPageViewModel.PageName, new OperatorPage());
         _pages.Add(SettingsPageViewModel.PageName, new SettingsPage());

         Messenger.Default.Send(new NavigateMessage(OperatorPageViewModel.PageName, null));

#pragma warning disable 4014
         // (fire and forget)
         LaunchTimerWindowAsync();
#pragma warning restore 4014
      }

      private void OnTimerMonitorChanged(TimerMonitorChangedMessage message)
      {
         if (_optionsService.IsTimerMonitorSpecified)
         {
            RelocateTimerWindow();
         }
         else
         {
            HideTimerWindow();
         }
      }

      private async Task LaunchTimerWindowAsync()
      {
         if (!IsInDesignMode && _optionsService.IsTimerMonitorSpecified)
         {
            await Task.Delay(1500);
            OpenTimerWindow();
         }
      }

      private void OnNavigate(NavigateMessage message)
      {
         CurrentPage = _pages[message.TargetPage];
         _currentPageName = message.TargetPage;
         ((IPage)CurrentPage.DataContext).Activated(message.State);
      }

      private FrameworkElement _currentPage;
      public FrameworkElement CurrentPage
      {
         get => _currentPage;
         set
         {
            if (!ReferenceEquals(_currentPage, value))
            {
               _currentPage = value;
               RaisePropertyChanged(nameof(CurrentPage));
            }
         }
      }

      public void Closing(object sender, CancelEventArgs e)
      {
         Messenger.Default.Send(new ShutDownMessage(_currentPageName));

         if(!e.Cancel)
         {
            CloseTimerWindow();
         }
      }

      private void RelocateTimerWindow()
      {
         if (_timerWindow != null)
         {
            var timerMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
            if (timerMonitor != null)
            {
               _timerWindow.Hide();
               _timerWindow.WindowState = WindowState.Normal;

               var area = timerMonitor.Monitor.WorkingArea;
               _timerWindow.Left = area.Left;
               _timerWindow.Top = area.Top;

               _timerWindow.Topmost = true;
               _timerWindow.WindowState = WindowState.Maximized;
               _timerWindow.Show();
            }
         }
      }
      
      private void OpenTimerWindow()
      {
         var timerMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
         if (timerMonitor != null)
         {
            _timerWindow = new TimerOutputWindow {DataContext = new TimerOutputWindowViewModel()};

            var area = timerMonitor.Monitor.WorkingArea;
            _timerWindow.Left = area.Left;
            _timerWindow.Top = area.Top;
            _timerWindow.Width = 0;
            _timerWindow.Height = 0;

            _timerWindow.Topmost = true;
            _timerWindow.Show();
         }
      }

      private void HideTimerWindow()
      {
         _timerWindow?.Hide();
      }

      private void CloseTimerWindow()
      {
         if (_timerWindow != null)
         {
            var model = (TimerOutputWindowViewModel) _timerWindow.DataContext;
            model.Cleanup();
            _timerWindow.Close();
            _timerWindow = null;
         }
      }
   }
}