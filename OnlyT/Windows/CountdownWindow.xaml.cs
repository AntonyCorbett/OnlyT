using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using OnlyT.ViewModel;

namespace OnlyT.Windows
{
    /// <summary>
    /// Interaction logic for CountdownWindow.xaml
    /// </summary>
    public partial class CountdownWindow : Window
    {
        public CountdownWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            var model = (CountdownTimerViewModel)DataContext;
            if (!model.ApplicationClosing)
            {
                // prevent window from being closed independently of application.
                e.Cancel = true;
            }
        }

        public void Start()
        {
            CountDown.Start(10);
        }
    }
}
