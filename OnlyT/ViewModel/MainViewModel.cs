using GalaSoft.MvvmLight;
using System.Windows;
using System.Collections.Generic;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.ViewModel.Messages;
using OnlyT.Windows;
using System.ComponentModel;
using System.Threading.Tasks;
using OnlyT.Services.Bell;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Services.Timer;

namespace OnlyT.ViewModel
{
   /// <summary>
   /// View model for the main page (which is a placeholder for the Operator or Settings page)
   /// </summary>
   public class MainViewModel : ViewModelBase
   {
      private readonly Dictionary<string, FrameworkElement> _pages = new Dictionary<string, FrameworkElement>();
      private TimerOutputWindow _timerWindow;
      private readonly IOptionsService _optionsService;
      private readonly IMonitorsService _monitorsService;
      private readonly IBellService _bellService;
      private readonly ITalkTimerService _timerService;
      private string _currentPageName;
      private readonly TimerOutputWindowViewModel _timerWindowViewModel;
      
      public MainViewModel(
         IOptionsService optionsService,
         IMonitorsService monitorsService,
         ITalkTimerService timerService,
         IBellService bellService)
      {
         _optionsService = optionsService;
         _monitorsService = monitorsService;
         _bellService = bellService;
         _timerService = timerService;

         // subscriptions...
         Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
         Messenger.Default.Register<TimerMonitorChangedMessage>(this, OnTimerMonitorChanged);
         Messenger.Default.Register<AlwaysOnTopChangedMessage>(this, OnAlwaysOnTopChanged);
         Messenger.Default.Register<OvertimeMessage>(this, OnTalkOvertime);

         // should really create a "page service" rather than create views in the main view model!
         _pages.Add(OperatorPageViewModel.PageName, new OperatorPage());
         _pages.Add(SettingsPageViewModel.PageName, new SettingsPage());

         _timerWindowViewModel = new TimerOutputWindowViewModel(_optionsService);

         Messenger.Default.Send(new NavigateMessage(OperatorPageViewModel.PageName, null));

#pragma warning disable 4014
         // (fire and forget)
         LaunchTimerWindowAsync();
#pragma warning restore 4014
      }

      private void OnTalkOvertime(OvertimeMessage message)
      {
         if (message.UseBellForTalk && _optionsService.Options.IsBellEnabled)
         {
            _bellService.Play(_optionsService.Options.BellVolumePercent);
         }
      }

      /// <summary>
      /// Responds to change in the application's "Always on top" option
      /// </summary>
      /// <param name="message"></param>
      private void OnAlwaysOnTopChanged(AlwaysOnTopChangedMessage message)
      {
         RaisePropertyChanged(nameof(AlwaysOnTop));
      }

      /// <summary>
      /// Responds to a change in timer monitor
      /// </summary>
      /// <param name="message"></param>
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
            // on launch we display the timer window after a short delay (for aesthetics only)
            await Task.Delay(1000).ConfigureAwait(true);
            OpenTimerWindow();
         }
      }

      /// <summary>
      /// Responds to the NavigateMessage and swaps out one page for another
      /// </summary>
      /// <param name="message"></param>
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

      public bool AlwaysOnTop => _optionsService.Options.AlwaysOnTop;

      public void Closing(object sender, CancelEventArgs e)
      {
         e.Cancel = _timerService.IsRunning;
         if(!e.Cancel)
         { 
            Messenger.Default.Send(new ShutDownMessage(_currentPageName));
            CloseTimerWindow();
         }
      }

      /// <summary>
      /// If the timer window is open when we change the timer display then relocate it;
      /// otherwise open it
      /// </summary>
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
         else
         {
            OpenTimerWindow();
         }
      }

      private void OpenTimerWindow()
      {
         var timerMonitor = _monitorsService.GetMonitorItem(_optionsService.Options.TimerMonitorId);
         if (timerMonitor != null)
         {
            _timerWindow = new TimerOutputWindow { DataContext = _timerWindowViewModel };

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
            _timerWindow.Close();
            _timerWindow = null;
         }
      }
   }
}