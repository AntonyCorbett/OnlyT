using System.Windows;

namespace OnlyT.Windows
{
    using System;
    using System.Windows.Media.Animation;

    using GalaSoft.MvvmLight.Messaging;

    using OnlyT.ViewModel.Messages;

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
