using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using GalaSoft.MvvmLight.Threading;
using OnlyT.ViewModel;

namespace OnlyT.Windows
{
    /// <summary>
    /// Interaction logic for CountdownWindow.xaml
    /// </summary>
    public partial class CountdownWindow : Window
    {
        public event EventHandler TimeUpEvent;
        
        public CountdownWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        public void Start(int offsetSeconds)
        {
            CountDown.Start(offsetSeconds);
        }

        private void OnCountDownTimeUp(object sender, System.EventArgs e)
        {
            CountDown.Stop();

            Task.Delay(1000).ContinueWith(t =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(OnTimeUpEvent);
            });
        }

        private void OnTimeUpEvent()
        {
            TimeUpEvent?.Invoke(this, System.EventArgs.Empty);
        }
    }
}
