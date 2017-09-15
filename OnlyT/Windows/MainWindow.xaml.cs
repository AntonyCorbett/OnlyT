using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using OnlyT.Services.Options;
using OnlyT.Utils;
using OnlyT.ViewModel;
using OnlyT.ViewModel.Messages;

namespace OnlyT.Windows
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      public MainWindow()
      {
         InitializeComponent();
         Messenger.Default.Register<TimerMonitorChangedMessage>(this, BringToFront);
      }

      private void BringToFront(TimerMonitorChangedMessage obj)
      {
         Activate();
      }

      protected override void OnSourceInitialized(System.EventArgs e)
      {
         var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
         if (!string.IsNullOrEmpty(optionsService.Options.AppWindowPlacement))
         {
            this.SetPlacement(optionsService.Options.AppWindowPlacement);
         }
      }

      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         SaveWindowPos();
         MainViewModel m = (MainViewModel)DataContext;
         m.Closing(sender, e);
      }

      private void SaveWindowPos()
      {
         var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
         optionsService.Options.AppWindowPlacement = this.GetPlacement();
      }

   }
}
