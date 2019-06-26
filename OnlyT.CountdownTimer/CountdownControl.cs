namespace OnlyT.CountdownTimer
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class CountdownControl : Control
    {
        private const int DefaultCountdownDurationMins = 5;

        private readonly DispatcherTimer _timer;
        private Path _donut;
        private Path _pie;
        private Ellipse _secondsBall;
        private TextBlock _time;
        private bool _registeredNames;
        private double _canvasWidth;
        private double _canvasHeight;
        private int _outerCircleDiameter;
        private int _outerCircleRadius;
        private int _innerCircleRadius;
        private Point _centrePoint;
        private DateTime _start;
        private double _pixelsPerDip;
        private int _countdownDurationMins = DefaultCountdownDurationMins;
        private bool _twoDigitMins;

        public static readonly DependencyProperty CountdownDurationMinsProperty =
            DependencyProperty.Register(
                "CountdownDurationMins",
                typeof(int),
                typeof(CountdownControl),
                new FrameworkPropertyMetadata(CountdownDurationMinsPropertyChanged));

        static CountdownControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(CountdownControl), 
                new FrameworkPropertyMetadata(typeof(CountdownControl)));
        }

        public CountdownControl()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Render)
            {
                Interval = TimeSpan.FromMilliseconds(20)
            };
            
            _timer.Tick += TimerFire;
        }

        public event EventHandler TimeUpEvent;

        public int CountdownDurationMins
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (int)GetValue(CountdownDurationMinsProperty);
            set => SetValue(CountdownDurationMinsProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("CountdownCanvas") is Canvas canvas)
            {
                InitCanvas(canvas);
            }
        }

        public void Start(int secsElapsed)
        {
            _start = DateTime.UtcNow.AddSeconds(-secsElapsed);
            _timer.Start();
        }

        public void Stop()
        {
            Dispatcher.Invoke(() =>
            {
                _timer.Stop();

                Animations.FadeOut(
                    this, 
                    new FrameworkElement[] { _donut, _secondsBall, _pie, _time },
                    (sender, args) =>
                    {
                        Visibility = Visibility.Hidden;
                    });
            });
        }

        protected virtual void OnTimeUpEvent()
        {
            TimeUpEvent?.Invoke(this, EventArgs.Empty);
        }

        private static void CountdownDurationMinsPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var c = (CountdownControl)d;
            c._countdownDurationMins = (int)e.NewValue;
        }

        private void RenderPieSliceAndBall(double angle, double secondsElapsed)
        {
            if (!Dispatcher.HasShutdownStarted)
            {
                _pie.Data = PieSlice.Get(angle, _centrePoint, _innerCircleRadius, _outerCircleRadius);

                var ballPt = SecondsBall.GetPos(
                    _centrePoint,
                    (int)secondsElapsed % 60,
                    _secondsBall.Width / 2,
                    _innerCircleRadius);

                Canvas.SetLeft(_secondsBall, ballPt.X);
                Canvas.SetTop(_secondsBall, ballPt.Y);

                _time.Text = GetTimeText();
            }
        }

        private void TimerFire(object sender, EventArgs e)
        {
            _timer.Stop();

            if (_start != default(DateTime))
            {
                var secsInCountdown = _countdownDurationMins * 60;
                var secondsElapsed = (DateTime.UtcNow - _start).TotalSeconds;
                var secondsLeft = secsInCountdown - secondsElapsed;

                if (secondsLeft >= 0)
                {
                    var angle = 360 - (((double)360 / secsInCountdown) * secondsLeft);
                    RenderPieSliceAndBall(angle, secondsElapsed);

                    if (!Dispatcher.HasShutdownStarted)
                    {
                        _timer.Start();
                    }
                }
                else
                {
                    RenderPieSliceAndBall(0, 0);

                    if (!Dispatcher.HasShutdownStarted)
                    {
                        OnTimeUpEvent();
                    }
                }
            }
            else
            {
                _timer.Start();
            }
        }

        private void InitCanvas(Canvas canvas)
        {
            AddGeometryToCanvas(canvas);
            canvas.Loaded += OnCanvasLoaded;
        }

        private void RegisterName(Canvas canvas, string name, object scopedElement)
        {
            if (canvas.FindName(name) == null)
            {
                RegisterName(name, scopedElement);
            }
        }

        private void RegisterNames(Canvas canvas)
        {
            if (!_registeredNames)
            {
                _registeredNames = true;

                RegisterName(canvas, _donut.Name, _donut);
                RegisterName(canvas, _pie.Name, _pie);
                RegisterName(canvas, _secondsBall.Name, _secondsBall);
                RegisterName(canvas, _time.Name, _time);
            }
        }

        private void OnCanvasLoaded(object sender, RoutedEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            
            RegisterNames(canvas);

            _pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            _canvasWidth = canvas.ActualWidth;
            _canvasHeight = canvas.ActualHeight;

            _twoDigitMins = _countdownDurationMins > 9;

            _time.FontSize = 12;
            _time.FontWeight = FontWeights.Bold;

            double outerRadiusFactor = 0.24;
            double innerRadiusFactor = 0.15;

            _outerCircleRadius = (int)(_canvasHeight * outerRadiusFactor);
            _outerCircleDiameter = _outerCircleRadius * 2;

            _innerCircleRadius = (int)(_canvasHeight * innerRadiusFactor);

            var totalWidthNeeded = 3.25 * _outerCircleDiameter;

            _centrePoint = CalcCentrePoint(totalWidthNeeded);

            _donut.Data = PieSlice.Get(0.1, _centrePoint, _innerCircleRadius, _outerCircleRadius);

            _secondsBall.Width = (double)_innerCircleRadius / 6;
            _secondsBall.Height = (double)_innerCircleRadius / 6;

            if (_countdownDurationMins == 1)
            {
                _secondsBall.Visibility = Visibility.Hidden;
            }

            _time.Text = GetTimeText();
            
            var sz = GetTextSize(_time.Text, useExtent: true);
            var szFactor = _twoDigitMins
                ? 0.60
                : 0.75;

            while (sz.Height < szFactor * _outerCircleDiameter)
            {
                _time.FontSize += 0.5;
                sz = GetTextSize(_time.Text, useExtent: true);
            }

            sz = GetTextSize(_time.Text, useExtent: true);

            Canvas.SetLeft(_time, _centrePoint.X - _outerCircleRadius + totalWidthNeeded - sz.Width);
            Canvas.SetTop(_time, _centrePoint.Y - sz.Height);

            RenderPieSliceAndBall(0.1, 0);

            Visibility = Visibility.Visible;
            Animations.FadeIn(this, new FrameworkElement[] { _donut, _secondsBall, _pie, _time });
        }

        private Color ToColor(string htmlColor)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (Color)ColorConverter.ConvertFromString(htmlColor);
        }

        private void AddGeometryToCanvas(Canvas canvas)
        {
            Color revealedColor = ToColor("#c0c5c1");
            Color externalRingColor = ToColor("#74546a");
            Color ballColor = ToColor("#eaf0ce");
            Color ringStrokeColor = ToColor("#473341");
            Color innerHighlightColor = Colors.White;

            var gs1 = new GradientStopCollection(2)
            {
                new GradientStop(innerHighlightColor, 0.7),
                new GradientStop(revealedColor, 1)
            };

            _donut = new Path
            {
                Fill = new RadialGradientBrush(gs1),
                Name = "Donut"
            };

            _pie = new Path
            {
                Stroke = new SolidColorBrush(ringStrokeColor),
                StrokeThickness = 1,
                Fill = new SolidColorBrush(externalRingColor),
                Name = "Pie"
            };

            _secondsBall = new Ellipse
            {
                Fill = new SolidColorBrush(ballColor),
                Name = "SecondsBall"
            };

            canvas.Children.Add(_donut);
            canvas.Children.Add(_pie);
            canvas.Children.Add(_secondsBall);
            
            Color textColor = ToColor("#eaf0ce");
            _time = new TextBlock { Foreground = new SolidColorBrush(textColor), Name = "TimeTxt" };
            canvas.Children.Add(_time);
        }

        private Point CalcCentrePoint(double totalWidthNeeded)
        {
            var margin = (_canvasWidth - totalWidthNeeded) / 2;
            return new Point(margin + _outerCircleRadius, _canvasHeight / 2);
        }

        private string GetTimeText(int? secsLeft = null)
        {
            double secondsLeft = secsLeft ?? 0;

            if (secsLeft == null)
            {
                if (_start == default(DateTime))
                {
                    secondsLeft = _countdownDurationMins * 60;
                }
                else
                {
                    var secsInCountdown = _countdownDurationMins * 60;
                    var secondsElapsed = (DateTime.UtcNow - _start).TotalSeconds;
                    secondsLeft = secsInCountdown - secondsElapsed + 1;
                }
            }

            int mins = (int)secondsLeft / 60;
            int secs = (int)(secondsLeft % 60);

            return _twoDigitMins
                ? $"{mins:D2}:{secs:D2}"
                : $"{mins}:{secs:D2}";
        }

        private Size GetTextSize(string text, bool useExtent)
        {
            var formattedText = new FormattedText(
                text, 
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, 
                new Typeface(_time.FontFamily, _time.FontStyle, _time.FontWeight, FontStretches.Normal),
                _time.FontSize,
                Brushes.Black,
                _pixelsPerDip);

            return new Size(formattedText.Width, useExtent ? formattedText.Extent : formattedText.Height);
        }
    }
}
