namespace OnlyT.Windows
{
    using System;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.CountdownTimer;
    using OnlyT.Services.Options;
    using OnlyT.Utils;
    using OnlyT.ViewModel;

    /// <summary>
    /// Interaction logic for CountdownWindow.xaml
    /// </summary>
    public partial class CountdownWindow : Window
    {
        private const double DefWindowWidth = 700;
        private const double DefWindowHeight = 500;
        
        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;

        public CountdownWindow(
            IOptionsService optionsService,
            IDateTimeService dateTimeService)
        {
            InitializeComponent();

            _optionsService = optionsService;
            _dateTimeService = dateTimeService;
        }

        public event EventHandler TimeUpEvent;
        
        public void AdjustWindowPositionAndSize()
        {
            if (!string.IsNullOrEmpty(_optionsService.Options.CountdownOutputWindowPlacement))
            {
                this.SetPlacement(
                    _optionsService.Options.CountdownOutputWindowPlacement, 
                    new Size(DefWindowWidth, DefWindowHeight));

                SetWindowSize();
            }
            else
            {
                Left = 10;
                Top = 10;
                Width = DefWindowWidth;
                Height = DefWindowHeight;
            }
        }
        
        public void SaveWindowPos()
        {
            _optionsService.Options.CountdownOutputWindowPlacement = this.GetPlacement();
            _optionsService.Options.CountdownWindowSize = new Size(Width, Height);
            _optionsService.Save();
        }

        public void Start(int offsetSeconds)
        {
            CountDown.Start(offsetSeconds);
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

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            var model = (CountdownTimerViewModel)DataContext;

            if (model.WindowedOperation)
            {
                SaveWindowPos();
            }
        }

        private void SetWindowSize()
        {
            var sz = _optionsService.Options.CountdownWindowSize;
            if (sz != default)
            {
                Width = sz.Width;
                Height = sz.Height;
            }
            else
            {
                Width = DefWindowWidth;
                Height = DefWindowHeight;
            }
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var isWindowed = ((CountdownTimerViewModel)DataContext).WindowedOperation;

            // allow drag when no title bar is shown
            if (isWindowed && e.ChangedButton == MouseButton.Left && WindowStyle == WindowStyle.None)
            {
                DragMove();
            }
        }
    }
}
