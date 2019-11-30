namespace OnlyT.Windows
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using GalaSoft.MvvmLight.Threading;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.CountdownTimer;

    /// <summary>
    /// Interaction logic for CountdownWindow.xaml
    /// </summary>
    public partial class CountdownWindow : Window
    {
        private readonly IDateTimeService _dateTimeService;

        public CountdownWindow(IDateTimeService dateTimeService)
        {
            InitializeComponent();

            _dateTimeService = dateTimeService;
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

        private void CountDownQueryUtcDateTime(object sender, UtcDateTimeQueryEventArgs e)
        {
            e.UtcDateTime = _dateTimeService.UtcNow();
        }
    }
}
