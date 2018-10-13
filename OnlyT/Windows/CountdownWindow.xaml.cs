namespace OnlyT.Windows
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using GalaSoft.MvvmLight.Threading;

    /// <summary>
    /// Interaction logic for CountdownWindow.xaml
    /// </summary>
    public partial class CountdownWindow : Window
    {
        public CountdownWindow()
        {
            InitializeComponent();
        }

        public event EventHandler TimeUpEvent;

        public void Start(int offsetSeconds)
        {
            CountDown.Start(offsetSeconds);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void OnCountDownTimeUp(object sender, EventArgs e)
        {
            CountDown.Stop();

            Task.Delay(1000).ContinueWith(t =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(OnTimeUpEvent);
            });
        }

        private void OnTimeUpEvent()
        {
            TimeUpEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
