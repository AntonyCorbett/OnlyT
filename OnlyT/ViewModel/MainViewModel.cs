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

namespace OnlyT.ViewModel
{
   public class MainViewModel : ViewModelBase
   {
      private Dictionary<string, FrameworkElement> _pages = new Dictionary<string, FrameworkElement>();
      private TimerOutputWindow _timerWindow;
      private string _currentPageName;


      public MainViewModel(ITalkTimerService timerService)
      {
         Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
         _pages.Add(OperatorPageViewModel.PageName, new OperatorPage());
         
         Messenger.Default.Send(new NavigateMessage(OperatorPageViewModel.PageName, null));
         OpenTimerWindow();
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
         get
         {
            return _currentPage;
         }
         set
         {
            if (_currentPage != value)
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
      
      private void OpenTimerWindow()
      {
         _timerWindow = new TimerOutputWindow();
         _timerWindow.DataContext = new TimerOutputWindowViewModel();

         var area = System.Windows.Forms.Screen.AllScreens[1].WorkingArea;
         _timerWindow.Left = area.Left;
         _timerWindow.Top = area.Top;
         _timerWindow.Topmost = true;
         _timerWindow.Show();
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