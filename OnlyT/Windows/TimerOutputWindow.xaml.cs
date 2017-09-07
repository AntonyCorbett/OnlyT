using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OnlyT
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

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         WindowState = WindowState.Maximized;
         TheClock.IsRunning = true;
      }
   }
}
