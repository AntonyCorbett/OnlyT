using System.Windows;
using OnlyT.ViewModel;

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
      }

      private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
      {
         MainViewModel m = (MainViewModel)DataContext;
         m.Closing(sender, e);
      }
   }
}
