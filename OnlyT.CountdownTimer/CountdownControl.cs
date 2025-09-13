﻿namespace OnlyT.CountdownTimer
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
        public static readonly DependencyProperty CountdownDurationMinsProperty =
            DependencyProperty.Register(
                nameof(CountdownDurationMins),
                typeof(int),
                typeof(CountdownControl),
                new FrameworkPropertyMetadata(CountdownDurationMinsPropertyChanged));

        public static readonly DependencyProperty ElementsToShowProperty =
            DependencyProperty.Register(
                nameof(ElementsToShow),
                typeof(ElementsToShow),
                typeof(CountdownControl),
                new FrameworkPropertyMetadata(ElementsToShowPropertyChanged));

        private const int DefaultCountdownDurationMins = 5;

        private DispatcherTimer? _timer;   // must be able to set to null in dispose.

        private Path? _donut;
        private Path? _pie;
        private Ellipse? _secondsBall;
        private TextBlock? _time;
        private bool _registeredNames;
        private double _canvasWidth;
        private double _canvasHeight;
        private AnnulusSize? _annulusSize;
        private Point _centrePointOfDial;
        private DateTime _start;
        private double _pixelsPerDip;
        private int _countdownDurationMins = DefaultCountdownDurationMins;
        private bool _twoDigitMins;
        private ElementsToShow _elementsToShow = ElementsToShow.DialAndDigital;

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

            RegisterToCleanupWhenParentCloses();
        }

        public event EventHandler<UtcDateTimeQueryEventArgs>? QueryUtcDateTimeEvent;

        public event EventHandler? TimeUpEvent;

        public int CountdownDurationMins
        {
            get => (int)GetValue(CountdownDurationMinsProperty);
            set => SetValue(CountdownDurationMinsProperty, value);
        }

        public ElementsToShow ElementsToShow
        {
            get => (ElementsToShow)GetValue(ElementsToShowProperty);
            set => SetValue(ElementsToShowProperty, value);
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
            _start = GetNowUtc().AddSeconds(-secsElapsed);
            _timer?.Start();
        }

        public void Stop()
        {
            Dispatcher.Invoke(() =>
            {
                if (_donut == null || _secondsBall == null || _pie == null || _time == null)
                {
                    return;
                }

                _timer?.Stop();

                Animations.FadeOut(
                    this,
                    [_donut, _secondsBall, _pie, _time],
                    (_, _) => Visibility = Visibility.Hidden);
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

        private static void ElementsToShowPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var c = (CountdownControl)d;
            c._elementsToShow = (ElementsToShow)e.NewValue;

            if (c.GetTemplateChild("CountdownCanvas") is Canvas canvas)
            {
                c.InitLayout(canvas);
            }
        }

        private void RenderPieSliceAndBall(double angle, double secondsElapsed)
        {
            if(_pie == null || _secondsBall == null || _time == null)
            {
                return;
            }

            if (!Dispatcher.HasShutdownStarted && IsLoaded && _annulusSize != null)
            {
                _pie.Data = PieSlice.Get(angle, _centrePointOfDial, _annulusSize.InnerRadius, _annulusSize.OuterRadius);

                var ballPt = SecondsBall.GetPos(
                    _centrePointOfDial,
                    (int)secondsElapsed % 60,
                    _secondsBall.Width / 2,
                    _annulusSize.InnerRadius);

                Canvas.SetLeft(_secondsBall, ballPt.X);
                Canvas.SetTop(_secondsBall, ballPt.Y);

                _time.Text = GetTimeText();
            }
        }

        private void TimerFire(object? sender, EventArgs e)
        {
            _timer?.Stop();

            if (_start != default)
            {
                var secsInCountdown = _countdownDurationMins * 60;
                var secondsElapsed = (GetNowUtc() - _start).TotalSeconds;
                var secondsLeft = secsInCountdown - secondsElapsed;

                if (secondsLeft >= 0)
                {
                    var angle = 360 - (((double)360 / secsInCountdown) * secondsLeft);
                    RenderPieSliceAndBall(angle, secondsElapsed);

                    if (!Dispatcher.HasShutdownStarted)
                    {
                        _timer?.Start();
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
                _timer?.Start();
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

        private void CheckVisualElements()
        {
            if (_donut == null || _pie == null || _secondsBall == null || _time == null)
            {
                throw new NotSupportedException("Visual elements not instantiated!");
            }
        }

        private void RegisterNames(Canvas canvas)
        {
            if (!_registeredNames)
            {
                CheckVisualElements();

                _registeredNames = true;

                RegisterName(canvas, _donut!.Name, _donut);
                RegisterName(canvas, _pie!.Name, _pie);
                RegisterName(canvas, _secondsBall!.Name, _secondsBall);
                RegisterName(canvas, _time!.Name, _time);
            }
        }

        private void OnCanvasLoaded(object sender, RoutedEventArgs e)
        {
            var canvas = (Canvas)sender;
            
            RegisterNames(canvas);

            InitLayout(canvas);
            CheckVisualElements();

            Visibility = Visibility.Visible;
            Animations.FadeIn(this, [_donut!, _secondsBall!, _pie!, _time!]);
        }

        private void InitLayout(Canvas canvas)
        {
            _pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

            _canvasWidth = canvas.ActualWidth;
            _canvasHeight = canvas.ActualHeight;

            _twoDigitMins = _countdownDurationMins > 9;

            _annulusSize = GetAnnulusSize();

            CheckVisualElements();

            _time!.FontSize = 12;
            _time.FontWeight = FontWeights.Bold;

            switch (_elementsToShow)
            {
                case ElementsToShow.Digital:
                    InitLayoutJustDigital();
                    break;

                case ElementsToShow.Dial:
                    InitLayoutJustDial();
                    break;

                case ElementsToShow.DialAndDigital:
                    InitLayoutDigitalAndDial();
                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        private void InitLayoutDigitalAndDial()
        {
            CheckVisualElements();

            var totalWidthNeeded = 3.25 * _annulusSize!.OuterDiameter;

            _centrePointOfDial = CalcCentrePointOfDIal(totalWidthNeeded);

            _donut!.Data = PieSlice.Get(0.1, _centrePointOfDial, _annulusSize.InnerRadius, _annulusSize.OuterRadius);

            _secondsBall!.Width = (double)_annulusSize.InnerRadius / 6;
            _secondsBall.Height = (double)_annulusSize.InnerRadius / 6;

            SetElementsVisibility();

            _time!.Text = GetTimeText();

            var sz = CalculateTextFontSizeWithDial();

            Canvas.SetLeft(_time, _centrePointOfDial.X - _annulusSize.OuterRadius + totalWidthNeeded - sz.Width);

            Canvas.SetTop(_time, _centrePointOfDial.Y - sz.Height);

            RenderPieSliceAndBall(0.1, 0);
        }

        private void InitLayoutJustDial()
        {
            CheckVisualElements();

            var totalWidthNeeded = _annulusSize!.OuterDiameter;

            _centrePointOfDial = CalcCentrePointOfDIal(totalWidthNeeded);

            _donut!.Data = PieSlice.Get(0.1, _centrePointOfDial, _annulusSize.InnerRadius, _annulusSize.OuterRadius);

            _secondsBall!.Width = (double)_annulusSize.InnerRadius / 6;
            _secondsBall.Height = (double)_annulusSize.InnerRadius / 6;

            SetElementsVisibility();
            
            RenderPieSliceAndBall(0.1, 0);
        }

        private void InitLayoutJustDigital()
        {
            SetElementsVisibility();
            CheckVisualElements();

            _time!.Text = GetTimeText();

            var sz = CalculateTextFontSizeNoDial();

            Canvas.SetLeft(_time, (_canvasWidth - sz.Width) / 2);
            Canvas.SetTop(_time, (_canvasHeight - sz.Height) / 2);
        }

        private Size CalculateTextFontSizeNoDial()
        {
            SetElementsVisibility();
            var sz = GetTextSize(_time!.Text, useExtent: false);

            const double szFactor = 0.95;

            while (sz.Width < szFactor * _canvasWidth)
            {
                _time.FontSize += 0.5;
                sz = GetTextSize(_time.Text, useExtent: false);
            }

            return GetTextSize(_time.Text, useExtent: false);
        }

        private Size CalculateTextFontSizeWithDial()
        {
            SetElementsVisibility();
            var sz = GetTextSize(_time!.Text, useExtent: true);
            var szFactor = _twoDigitMins
                    ? 0.60
                    : 0.75;

            while (sz.Height < szFactor * _annulusSize!.OuterDiameter)
            {
                _time.FontSize += 0.5;
                sz = GetTextSize(_time.Text, useExtent: true);
            }

            return GetTextSize(_time.Text, useExtent: true);
        }

        private AnnulusSize GetAnnulusSize()
        {
            var outerRadiusFactor = 0.24;
            var innerRadiusFactor = 0.15;

            switch (_elementsToShow)
            {
                case ElementsToShow.Dial:
                    outerRadiusFactor *= 1.75;
                    innerRadiusFactor *= 1.75;
                    break;

                default:
                    // no adjustment needed
                    break;
            }

            return new AnnulusSize
            {
                InnerRadius = (int)(_canvasHeight * innerRadiusFactor),
                OuterRadius = (int)(_canvasHeight * outerRadiusFactor)
            };
        }

        private void SetElementsVisibility()
        {
            CheckVisualElements();

            switch (_elementsToShow)
            {
                case ElementsToShow.DialAndDigital:
                    _secondsBall!.Visibility = Visibility.Visible;
                    _donut!.Visibility = Visibility.Visible;
                    _time!.Visibility = Visibility.Visible;
                    _pie!.Visibility = Visibility.Visible;
                    break;

                case ElementsToShow.Dial:
                    _secondsBall!.Visibility = Visibility.Visible;
                    _donut!.Visibility = Visibility.Visible;
                    _pie!.Visibility = Visibility.Visible;
                    _time!.Visibility = Visibility.Hidden;
                    break;

                case ElementsToShow.Digital:
                    _time!.Visibility = Visibility.Visible;
                    _secondsBall!.Visibility = Visibility.Hidden;
                    _donut!.Visibility = Visibility.Hidden;
                    _pie!.Visibility = Visibility.Hidden;
                    break;

                default:
                    throw new NotSupportedException();
            }

            if (_countdownDurationMins == 1)
            {
                // gratuitous
                _secondsBall!.Visibility = Visibility.Hidden;
            }
        }

        private static Color ToColor(string htmlColor)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (Color)ColorConverter.ConvertFromString(htmlColor);
        }

        private void AddGeometryToCanvas(Canvas canvas)
        {
            var revealedColor = ToColor("#c0c5c1");
            var externalRingColor = ToColor("#74546a");
            var ballColor = ToColor("#eaf0ce");
            var ringStrokeColor = ToColor("#473341");
            var innerHighlightColor = Colors.White;

            var gs1 = new GradientStopCollection(2)
            {
                new(innerHighlightColor, 0.7),
                new(revealedColor, 1)
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
            
            var textColor = ToColor("#eaf0ce");
            _time = new TextBlock { Foreground = new SolidColorBrush(textColor), Name = "TimeTxt" };
            canvas.Children.Add(_time);
        }

        private Point CalcCentrePointOfDIal(double totalWidthNeeded)
        {
            SetElementsVisibility();

            var margin = (_canvasWidth - totalWidthNeeded) / 2;
            return new Point(margin + _annulusSize!.OuterRadius, _canvasHeight / 2);
        }

        private string GetTimeText(int? secsLeft = null)
        {
            double secondsLeft = secsLeft ?? 0;

            if (secsLeft == null)
            {
                if (_start == default)
                {
                    secondsLeft = _countdownDurationMins * 60;
                }
                else
                {
                    var secsInCountdown = _countdownDurationMins * 60;
                    var secondsElapsed = (GetNowUtc() - _start).TotalSeconds;
                    secondsLeft = secsInCountdown - secondsElapsed + 1;
                }
            }

            var mins = (int)secondsLeft / 60;
            var secs = (int)(secondsLeft % 60);

            return _twoDigitMins
                ? $"{mins:D2}:{secs:D2}"
                : $"{mins}:{secs:D2}";
        }

        private Size GetTextSize(string text, bool useExtent)
        {
            CheckVisualElements();

            var formattedText = new FormattedText(
                text, 
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, 
                new Typeface(_time!.FontFamily, _time.FontStyle, _time.FontWeight, FontStretches.Normal),
                _time.FontSize,
                Brushes.Black,
                _pixelsPerDip);

            return new Size(formattedText.Width, useExtent ? formattedText.Extent : formattedText.Height);
        }

        private DateTime GetNowUtc()
        {
            var args = new UtcDateTimeQueryEventArgs();
            OnQueryDateTime(args);
            return args.UtcDateTime;
        }

        private void OnQueryDateTime(UtcDateTimeQueryEventArgs e)
        {
            if (QueryUtcDateTimeEvent == null)
            {
                e.UtcDateTime = DateTime.UtcNow;
            }

            QueryUtcDateTimeEvent?.Invoke(this, e);
        }

        private void RegisterToCleanupWhenParentCloses()
        {
            // get opportunity to clear the timer event handler
            // when the parent window closes.
            Loaded += (_, _) =>
            {
                var parent = Window.GetWindow(this);
                if (parent != null)
                {
                    parent.Closing += (_, _) => DisposeLogic();
                }
            };
        }

        private void DisposeLogic()
        {
            if (_timer == null)
            {
                return;
            }

            // clear the handler 
            _timer!.Tick -= TimerFire;

            // and break the retention link to the global dispatch timer list
            _timer = null;
        }
    }
}
