namespace OnlyT.AnalogueClock
{
    // ReSharper disable UnusedMember.Global
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    public sealed class ClockControl : Control
    {
        public static readonly DependencyProperty IsFlatProperty =
            DependencyProperty.Register(
                "IsFlat",
                typeof(bool),
                typeof(ClockControl),
                new FrameworkPropertyMetadata(IsFlatPropertyChanged));

        public static readonly DependencyProperty DigitalTimeFormatShowLeadingZeroProperty =
            DependencyProperty.Register(
                "DigitalTimeFormatShowLeadingZero", 
                typeof(bool), 
                typeof(ClockControl),
                new FrameworkPropertyMetadata(DigitalTimeFormatShowLeadingZeroPropertyChanged));

        public static readonly DependencyProperty DigitalTimeFormat24HoursProperty =
            DependencyProperty.Register(
                "DigitalTimeFormat24Hours", 
                typeof(bool), 
                typeof(ClockControl),
                new FrameworkPropertyMetadata(DigitalTimeFormat24HoursPropertyChanged));

        public static readonly DependencyProperty DigitalTimeFormatAMPMProperty =
            DependencyProperty.Register(
                "DigitalTimeFormatAMPM", 
                typeof(bool), 
                typeof(ClockControl),
                new FrameworkPropertyMetadata(DigitalTimeFormatAMPMPropertyChanged));

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register(
                "IsRunning", 
                typeof(bool), 
                typeof(ClockControl),
                new FrameworkPropertyMetadata(IsRunningPropertyChanged));

        public static readonly DependencyProperty CurrentTimeHrMinProperty =
            DependencyProperty.Register(
                "CurrentTimeHrMin", 
                typeof(string), 
                typeof(ClockControl));

        public static readonly DependencyProperty CurrentTimeSecProperty =
            DependencyProperty.Register(
                "CurrentTimeSec", 
                typeof(string), 
                typeof(ClockControl));

        public static readonly DependencyProperty DurationSectorProperty =
            DependencyProperty.Register(
                "DurationSector", 
                typeof(DurationSector), 
                typeof(ClockControl),
                new FrameworkPropertyMetadata(DurationSectorPropertyChanged));
        
        private static readonly double ClockRadius = 250;
        private static readonly double SectorRadius = 230;
        private static readonly Point ClockOrigin = new Point(ClockRadius, ClockRadius);
        private static readonly TimeSpan TimerInterval = TimeSpan.FromMilliseconds(100);
        private static readonly TimeSpan AnimationTimerInterval = TimeSpan.FromMilliseconds(20);
        private static readonly double AngleTolerance = 0.75;

        private readonly DispatcherTimer _timer;
        private readonly DispatcherTimer _animationTimer;

        private AnglesOfHands _animationTargetAngles;
        private AnglesOfHands _animationCurrentAngles;

        private double _hourHandDropShadowOpacity;
        private double _minuteHandDropShadowOpacity;
        private double _secondHandDropShadowOpacity;

        private bool _elementsAvailable;
        private bool _showElapsedSector;
        private Ellipse _outerDial;
        private Ellipse _middleDial;
        private Ellipse _centrePointDial;
        private Line _minuteHand;
        private Line _hourHand;
        private Line _secondHand;
        private Path _sectorPath1;
        private Path _sectorPath2;
        private Path _sectorPath3;
        private bool _digitalFormatLeadingZero;
        private bool _digitalFormat24Hours;
        private bool _digitalFormatAMPM;
        
        static ClockControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ClockControl),
                new FrameworkPropertyMetadata(typeof(ClockControl)));
        }

        public ClockControl()
        {
            var isInDesignMode = DesignerProperties.GetIsInDesignMode(this);

            _showElapsedSector = true;

            _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimerInterval };
            _timer.Tick += TimerCallback;

            _animationTimer = new DispatcherTimer(DispatcherPriority.Render) { Interval = AnimationTimerInterval };
            _animationTimer.Tick += AnimationCallback;

            if (isInDesignMode)
            {
                CurrentTimeHrMin = "3:50";
                CurrentTimeSec = "20";
            }
        }

        public event EventHandler<DateTimeQueryEventArgs> QueryDateTimeEvent;

        public bool DigitalTimeFormat24Hours
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (bool)GetValue(DigitalTimeFormat24HoursProperty);
            set => SetValue(DigitalTimeFormat24HoursProperty, value);
        }

        public bool DigitalTimeFormatAMPM
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (bool)GetValue(DigitalTimeFormatAMPMProperty);
            set => SetValue(DigitalTimeFormatAMPMProperty, value);
        }

        public bool IsRunning
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        public DurationSector DurationSector
        {
            get => (DurationSector)GetValue(DurationSectorProperty);
            set => SetValue(DurationSectorProperty, value);
        }

        public bool DigitalTimeFormatShowLeadingZero
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (bool)GetValue(DigitalTimeFormatShowLeadingZeroProperty);
            set => SetValue(DigitalTimeFormatShowLeadingZeroProperty, value);
        }

        public bool IsFlat
        {
            // ReSharper disable once PossibleNullReferenceException
            get => (bool)GetValue(IsFlatProperty);
            set => SetValue(IsFlatProperty, value);
        }

        public string CurrentTimeHrMin
        {
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once UnusedMember.Local
            get => (string)GetValue(CurrentTimeHrMinProperty);
            private set => SetValue(CurrentTimeHrMinProperty, value);
        }

        public string CurrentTimeSec
        {
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once UnusedMember.Local
            get => (string)GetValue(CurrentTimeSecProperty);
            private set => SetValue(CurrentTimeSecProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            GetElementRefs();

            SetFlatOrNonFlatStyle();
            
            if (GetTemplateChild("ClockCanvas") is Canvas cc)
            {
                GenerateHourMarkers(cc);
                GenerateHourNumbers(cc);
            }
        }

        private static void IsFlatPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var c = (ClockControl)d;
            c.SetFlatOrNonFlatStyle();
        }

        private static void DigitalTimeFormatShowLeadingZeroPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var c = (ClockControl)d;
            c._digitalFormatLeadingZero = (bool)e.NewValue;
        }

        private static void DigitalTimeFormat24HoursPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var c = (ClockControl)d;
            c._digitalFormat24Hours = (bool)e.NewValue;
        }

        private static void DigitalTimeFormatAMPMPropertyChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var c = (ClockControl)d;
            c._digitalFormatAMPM = (bool)e.NewValue;
        }

        private static void IsRunningPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var b = (bool)e.NewValue;
            var c = (ClockControl)d;
            if (b)
            {
                c.StartupAnimation();
            }
            else
            {
                c._timer.IsEnabled = false;
            }
        }

        private static void DurationSectorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sector = (DurationSector)e.NewValue;
            var c = (ClockControl)d;

            if (sector == null)
            {
                c._sectorPath1.Data = null;
                c._sectorPath2.Data = null;
                c._sectorPath3.Data = null;
            }
            else
            {
                var delay = e.OldValue == null;
                if (delay)
                {
                    // the first time we display the sector we delay it for aesthetics
                    // (so it doesn't appear just as the clock is fading out)
                    Task.Delay(1000).ContinueWith(t => { c.Dispatcher?.Invoke(() => { DrawSector(c, sector); }); });
                }
                else
                {
                    DrawSector(c, sector);
                }
            }
        }

        private static bool IsLargeArc(double startAngle, double endAngle)
        {
            if (endAngle < startAngle)
            {
                return (360 - startAngle + endAngle) > 180;
            }

            return endAngle - startAngle >= 180.0;
        }

        private static void DrawSector(ClockControl c, DurationSector sector)
        {
            // we may have 1, 2 or 3 sectors to draw...

            // green sector...
            if (!sector.IsOvertime)
            {
                DrawSector(c._sectorPath1, sector.StartAngle, sector.EndAngle, IsLargeArc(sector.StartAngle, sector.EndAngle));
            }

            c.SetShowElapsedSector(sector.ShowElapsedSector);

            // light green sector...
            DrawSector(c._sectorPath2, sector.StartAngle, sector.CurrentAngle, IsLargeArc(sector.StartAngle, sector.CurrentAngle));

            if (sector.IsOvertime)
            {
                // red sector...
                DrawSector(c._sectorPath3, sector.EndAngle, sector.CurrentAngle, IsLargeArc(sector.EndAngle, sector.CurrentAngle));
            }
        }

        private static void DrawSector(Path sectorPath, double startAngle, double endAngle, bool isLargeArc)
        {
            var startPoint = PointOnCircle(SectorRadius, startAngle, ClockOrigin);
            var endPoint = PointOnCircle(SectorRadius, endAngle, ClockOrigin);
            string largeArc = isLargeArc ? "1" : "0";

            // use InvariantCulture to ensure that decimal separator is '.'
            sectorPath.Data = Geometry.Parse($"M{ClockRadius.ToString(CultureInfo.InvariantCulture)},{ClockRadius.ToString(CultureInfo.InvariantCulture)} L{startPoint.X.ToString(CultureInfo.InvariantCulture)},{startPoint.Y.ToString(CultureInfo.InvariantCulture)} A{SectorRadius.ToString(CultureInfo.InvariantCulture)},{SectorRadius.ToString(CultureInfo.InvariantCulture)} 0 {largeArc} 1 {endPoint.X.ToString(CultureInfo.InvariantCulture)},{endPoint.Y.ToString(CultureInfo.InvariantCulture)} z");
        }

        private static Point PointOnCircle(double radius, double angleInDegrees, Point origin)
        {
            // NB - angleInDegrees is from 12 o'clock rather than from 3 o'clock
            var x = (radius * Math.Cos((angleInDegrees - 90) * Math.PI / 180F)) + origin.X;
            var y = (radius * Math.Sin((angleInDegrees - 90) * Math.PI / 180F)) + origin.Y;

            return new Point(x, y);
        }

        private double CalculateAngleSeconds(DateTime dt)
        {
            return dt.Second * 6;
        }

        private double CalculateAngleMinutes(DateTime dt)
        {
            return 
                (dt.Minute * 6) + 
                (dt.Second * 0.1) +
                (dt.Millisecond * 0.0001);
        }

        private double CalculateAngleHours(DateTime dt)
        {
            var hr = dt.Hour >= 12 ? dt.Hour - 12 : dt.Hour;
            return (hr * 30) + (((double)dt.Minute / 60) * 30);
        }

        private void PositionClockHand(Line hand, double angle, double shadowOpacity)
        {
            ((DropShadowEffect)hand.Effect).Opacity = shadowOpacity;
            ((DropShadowEffect)hand.Effect).Direction = angle;
            hand.RenderTransform = new RotateTransform(angle, ClockRadius, ClockRadius);
        }

        private void TimerCallback(object sender, EventArgs eventArgs)
        {
            var now = GetNow();
            
            PositionClockHand(_secondHand, CalculateAngleSeconds(now), _secondHandDropShadowOpacity);
            PositionClockHand(_minuteHand, CalculateAngleMinutes(now), _minuteHandDropShadowOpacity);
            PositionClockHand(_hourHand, CalculateAngleHours(now), _hourHandDropShadowOpacity);

            CurrentTimeHrMin = FormatTimeOfDayHoursAndMins(now);
            CurrentTimeSec = FormatTimeOfDaySeconds(now);
        }

        private string FormatTimeOfDayHoursAndMins(DateTime dt)
        {
            int hours = _digitalFormat24Hours 
                ? dt.Hour 
                : dt.Hour > 12 
                    ? dt.Hour - 12 
                    : dt.Hour;

            var ampm = GetAmPmPostfix(dt);

            if (_digitalFormatLeadingZero)
            {
                return $"{hours:D2}:{dt.Minute:D2}{ampm}";
            }

            return $"{hours}:{dt.Minute:D2}{ampm}";
        }

        private string GetAmPmPostfix(DateTime dt)
        {
            if (!_digitalFormatAMPM)
            {
                return string.Empty;
            }

            var ampm = dt.ToString("tt");

            if (string.IsNullOrEmpty(ampm))
            {
                return string.Empty;
            }

            return $" {ampm}";
        }

        private string FormatTimeOfDaySeconds(DateTime dt)
        {
            return _digitalFormatAMPM ? string.Empty : dt.Second.ToString("D2");
        }

        private void StartupAnimation()
        {
            _animationTargetAngles = GenerateTargetAngles();
            _animationCurrentAngles = new AnglesOfHands
            {
                HoursAngle = 0.0,
                MinutesAngle = 0.0,
                SecondsAngle = 0.0
            };
            _animationTimer.Start();
        }

        private AnglesOfHands GenerateTargetAngles()
        {
            var targetTime = GetNow();

            return new AnglesOfHands
            {
                SecondsAngle = CalculateAngleSeconds(targetTime),
                MinutesAngle = CalculateAngleMinutes(targetTime),
                HoursAngle = CalculateAngleHours(targetTime)
            };
        }

        private void AnimationCallback(object sender, EventArgs eventArgs)
        {
            ((DispatcherTimer)sender).Stop();

            _animationCurrentAngles.SecondsAngle = AnimateHand(
                _secondHand, 
                _animationCurrentAngles.SecondsAngle, 
                _animationTargetAngles.SecondsAngle,
                _secondHandDropShadowOpacity);

            _animationCurrentAngles.MinutesAngle = AnimateHand(
                _minuteHand, 
                _animationCurrentAngles.MinutesAngle, 
                _animationTargetAngles.MinutesAngle,
                _minuteHandDropShadowOpacity);

            _animationCurrentAngles.HoursAngle = AnimateHand(
                _hourHand, 
                _animationCurrentAngles.HoursAngle, 
                _animationTargetAngles.HoursAngle,
                _hourHandDropShadowOpacity);

            if (AnimationShouldContinue())
            {
                ((DispatcherTimer)sender).Start();
            }
            else
            {
                _timer.Start();
            }
        }

        private double AnimateHand(Line hand, double currentAngle, double targetAngle, double shadowOpacity)
        {
            if (Math.Abs(currentAngle - targetAngle) > AngleTolerance)
            {
                double delta = (targetAngle - currentAngle) / 5;
                currentAngle += delta;
                
                PositionClockHand(hand, currentAngle, shadowOpacity);
            }

            return currentAngle;
        }

        private bool AnimationShouldContinue()
        {
            return
               Math.Abs(_animationCurrentAngles.SecondsAngle - _animationTargetAngles.SecondsAngle) > AngleTolerance ||
               Math.Abs(_animationCurrentAngles.MinutesAngle - _animationTargetAngles.MinutesAngle) > AngleTolerance ||
               Math.Abs(_animationCurrentAngles.HoursAngle - _animationTargetAngles.HoursAngle) > AngleTolerance;
        }

        private TextBlock CreateHourNumberTextBlock(int hour)
        {
            return new TextBlock
            {
                Text = hour.ToString(),
                FontSize = 36,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.DarkSlateBlue
            };
        }

        private void GenerateHourNumbers(Canvas canvas)
        {
            double clockRadius = canvas.Width / 2;
            double centrePointRadius = clockRadius - 50;
            double borderSize = 80;

            for (int n = 0; n < 12; ++n)
            {
                var angle = n * 30;
                var pt = PointOnCircle(centrePointRadius, angle, ClockOrigin);

                var hour = n == 0 ? 12 : n;

                var t = CreateHourNumberTextBlock(hour);
                var b = new Border
                {
                    Width = borderSize,
                    Height = borderSize,
                    Child = t
                };

                canvas.Children.Add(b);
                Canvas.SetLeft(b, pt.X - (borderSize / 2));
                Canvas.SetTop(b, pt.Y - (borderSize / 2));
            }
        }

        private void GenerateHourMarkers(Canvas canvas)
        {
            var angle = 0;
            for (var n = 0; n < 4; ++n)
            {
                var line = CreateMajorHourMarker();
                line.RenderTransform = new RotateTransform(angle += 90, ClockRadius, ClockRadius);
                canvas.Children.Add(line);
            }

            for (var n = 0; n < 12; ++n)
            {
                if (n % 3 > 0)
                {
                    var line = CreateMinorHourMarker();
                    line.RenderTransform = new RotateTransform(angle, ClockRadius, ClockRadius);
                    canvas.Children.Add(line);
                }

                angle += 30;
            }

            for (var n = 0; n < 60; ++n)
            {
                if (n % 5 > 0)
                {
                    var line = CreateMinuteMarker();
                    line.RenderTransform = new RotateTransform(angle, ClockRadius, ClockRadius);
                    canvas.Children.Add(line);
                }

                angle += 6;
            }
        }

        private Line CreateMajorHourMarker()
        {
            return new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 7,
                X1 = ClockRadius,
                Y1 = 19,
                X2 = ClockRadius,
                Y2 = 27
            };
        }

        private Line CreateMinorHourMarker()
        {
            return new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 3,
                X1 = ClockRadius,
                Y1 = 19,
                X2 = ClockRadius,
                Y2 = 25
            };
        }

        private Line CreateMinuteMarker()
        {
            return new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                X1 = ClockRadius,
                Y1 = 19,
                X2 = ClockRadius,
                Y2 = 25
            };
        }

        private void GetElementRefs()
        {
            _outerDial = GetTemplateChild("OuterDial") as Ellipse;
            _middleDial = GetTemplateChild("MiddleDial") as Ellipse;
            _centrePointDial = GetTemplateChild("CentrePointDial") as Ellipse;

            if (GetTemplateChild("MinuteHand") is Line line1)
            {
                _minuteHand = line1;
                _minuteHand.Effect = new DropShadowEffect
                {
                    Color = Colors.DarkGray,
                    BlurRadius = 5,
                    ShadowDepth = 3,
                    Opacity = _minuteHandDropShadowOpacity
                };
            }

            if (GetTemplateChild("HourHand") is Line line2)
            {
                _hourHand = line2;
                _hourHand.Effect = new DropShadowEffect
                {
                    Color = Colors.DarkGray,
                    BlurRadius = 5,
                    ShadowDepth = 3,
                    Opacity = _hourHandDropShadowOpacity
                };
            }

            if (GetTemplateChild("SecondHand") is Line line3)
            {
                _secondHand = line3;
                _secondHand.Effect = new DropShadowEffect
                {
                    Color = Colors.DarkGray,
                    BlurRadius = 1.2,
                    ShadowDepth = 3,
                    Opacity = _secondHandDropShadowOpacity
                };
            }

            if (GetTemplateChild("SectorPath1") is Path sectorPath1)
            {
                _sectorPath1 = sectorPath1;
            }

            if (GetTemplateChild("SectorPath2") is Path sectorPath2)
            {
                _sectorPath2 = sectorPath2;
            }

            if (GetTemplateChild("SectorPath3") is Path sectorPath3)
            {
                _sectorPath3 = sectorPath3;
            }

            _elementsAvailable = true;
        }

        private void SetFlatOrNonFlatStyle()
        {
            if (_elementsAvailable)
            {
                if (IsFlat)
                {
                    SetFlatStyle();
                }
                else
                {
                    SetDefaultNonFlatStyle();
                }
            }
        }

        private void SetDefaultNonFlatStyle()
        {
            _outerDial.Fill = ClockBrushes.GetOuterDialBrush();
            _middleDial.Fill = ClockBrushes.GetMiddleDialBrush();
            _centrePointDial.Effect = ClockBrushes.GetCentrePointDropShadow();

            _secondHandDropShadowOpacity = 0.3;
            _hourHandDropShadowOpacity = 1.0;
            _minuteHandDropShadowOpacity = 0.4;
        }

        private void SetFlatStyle()
        {
            _outerDial.Fill = ClockBrushes.GetOuterDialBrushFlat();
            _middleDial.Fill = ClockBrushes.GetMiddleDialBrushFlat();
            _centrePointDial.Effect = null;

            _secondHandDropShadowOpacity = 0.0;
            _hourHandDropShadowOpacity = 0.0;
            _minuteHandDropShadowOpacity = 0.0;
        }

        private void SetShowElapsedSector(bool value)
        {
            if (_showElapsedSector != value)
            {
                _showElapsedSector = value;

                if (_showElapsedSector)
                {
                    _sectorPath2.Fill = new SolidColorBrush(Color.FromRgb(230, 255, 230));
                    _sectorPath2.StrokeThickness = 1;
                }
                else
                {
                    _sectorPath2.Fill = Brushes.White;
                    _sectorPath2.StrokeThickness = 0;
                }
            }
        }

        private DateTime GetNow()
        {
            var args = new DateTimeQueryEventArgs();
            OnQueryDateTime(args);
            return args.DateTime;
        }

        private void OnQueryDateTime(DateTimeQueryEventArgs e)
        {
            if (QueryDateTimeEvent == null)
            {
                e.DateTime = DateTime.Now;
            }

            QueryDateTimeEvent?.Invoke(this, e);
        }
    }
}
