using System.Windows;

namespace OnlyT.Windows
{
   /// <summary>
   /// Interaction logic for TimerOutputWindow.xaml
   /// </summary>
   public partial class TimerOutputWindow : Window
   {
      public TimerOutputWindow()
      {
         InitializeComponent();
      }

      private void WindowLoaded(object sender, RoutedEventArgs e)
      {
         WindowState = WindowState.Maximized;
         TheClock.IsRunning = true;
      }
   }
}
