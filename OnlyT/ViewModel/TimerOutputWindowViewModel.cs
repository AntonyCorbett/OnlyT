﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace OnlyT.ViewModel;

// ReSharper disable UnusedMember.Global
using System;
using System.Windows.Input;
using System.Windows.Media;
using AnalogueClock;
using Messages;
using OnlyT.Common.Services.DateTime;
using Services.Options;
using Utils;

// ReSharper disable once ClassNeverInstantiated.Global
public class TimerOutputWindowViewModel : ObservableObject
{
    private static readonly int _secsPerHour = 60 * 60;
    private readonly IOptionsService _optionsService;
    private readonly IDateTimeService _dateTimeService;
    
    private int _analogueClockColumnWidthPercentage = -1;
    private string? _timeString;
    private bool _isRunning;
    private Brush _textColor = GreenYellowRedSelector.GetGreenBrush();
    private DurationSector? _durationSector;
    private bool _showTimeOfDayUnderTimer;
    private bool _windowedOperation;

    public TimerOutputWindowViewModel(
        IOptionsService optionsService,
        IDateTimeService dateTimeService)
    {
        _optionsService = optionsService;
        _dateTimeService = dateTimeService;
        
        AnalogueClockColumnWidthPercentage = _optionsService.Options.AnalogueClockWidthPercent;
        ShowTimeOfDayUnderTimer = _optionsService.Options.ShowTimeOfDayUnderTimer;

        // subscriptions...
        WeakReferenceMessenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        WeakReferenceMessenger.Default.Register<TimerChangedMessage>(this, OnTimerChanged);
        WeakReferenceMessenger.Default.Register<TimerStartMessage>(this, OnTimerStarted);
        WeakReferenceMessenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
        WeakReferenceMessenger.Default.Register<ClockHourFormatChangedMessage>(this, OnDigitalClockFormatChanged);
        WeakReferenceMessenger.Default.Register<AnalogueClockWidthChangedMessage>(this, OnAnalogueClockWidthChanged);
        WeakReferenceMessenger.Default.Register<ShowTimeOfDayUnderTimerChangedMessage>(this, OnShowTimeOfDayUnderTimerChanged);
        WeakReferenceMessenger.Default.Register<MousePointerInTimerDisplayChangedMessage>(this, OnMousePointerChanged);
        WeakReferenceMessenger.Default.Register<TimerFrameChangedMessage>(this, OnTimerFrameChanged);
        WeakReferenceMessenger.Default.Register<ClockIsFlatChangedMessage>(this, OnClockIsFlatChanged);
    }

    public int BorderThickness => !WindowedOperation && _optionsService.Options.TimerFrame ? 3 : 0;
        
    public int TimerBorderThickness => !WindowedOperation && _optionsService.Options.ClockTimerFrame ? 3 : 0;

    public int TimerColumnWidthPercentage { get; private set; } = -1;

    public bool ApplicationClosing { get; private set; }

    public bool ClockIsFlat => WindowedOperation || _optionsService.Options.ClockIsFlat;

    public bool ShowTimeOfDayUnderTimer
    {
        get => _showTimeOfDayUnderTimer;
        set
        {
            if (_showTimeOfDayUnderTimer != value)
            {
                _showTimeOfDayUnderTimer = value;
                OnPropertyChanged();
            }
        }
    }

    public FullScreenClockMode FullScreenClockMode => _optionsService.Options.FullScreenClockMode;

    public Cursor MousePointer =>
        _optionsService.Options.ShowMousePointerInTimerDisplay
            ? Cursors.Arrow
            : Cursors.None;

    public string? TimeString
    {
        get => _timeString;
        set
        {
            if (_timeString != value)
            {
                _timeString = value;
                OnPropertyChanged();
            }
        }
    }
        
    public DurationSector? DurationSector
    {
        get => _durationSector;
        set
        {
            if (!ReferenceEquals(_durationSector, value))
            {
                _durationSector = value;
                OnPropertyChanged();
            }
        }
    }

    public Brush TextColor
    {
        get => _textColor;
        set
        {
            if (!ReferenceEquals(_textColor, value))
            {
                _textColor = value;
                OnPropertyChanged();
            }
        }
    }
        
    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (_isRunning != value)
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }
    }
    
    public const int NdiPixelHeight = 1080;

    public const int NdiPixelWidth = 1920;
    
    // don't make these static without altering the binding in xaml
#pragma warning disable CA1822
    public int NdiPixelHeightValue => NdiPixelHeight;
    public int NdiPixelWidthValue => NdiPixelWidth;
#pragma warning restore CA1822

    public int AnalogueClockColumnWidthPercentage
    {
        get => _analogueClockColumnWidthPercentage;
        set
        {
            if (_analogueClockColumnWidthPercentage != value)
            {
                _analogueClockColumnWidthPercentage = value;
                TimerColumnWidthPercentage = 100 - _analogueClockColumnWidthPercentage;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimerColumnWidthPercentage));
            }
        }
    }
        
    public bool DigitalTimeFormatShowLeadingZero =>
        _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZero ||
        _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZeroAMPM ||
        _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24LeadingZero;
        
    public bool DigitalTimeFormat24Hours =>
        _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24 ||
        _optionsService.Options.ClockHourFormat == ClockHourFormat.Format24LeadingZero;
        
    public bool DigitalTimeFormatAMPM =>
        _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12AMPM ||
        _optionsService.Options.ClockHourFormat == ClockHourFormat.Format12LeadingZeroAMPM;

    public bool DigitalTimeShowSeconds => _optionsService.Options.ShowDigitalSeconds;

    public bool UseTimerBackgroundGradient => !WindowedOperation && _optionsService.Options.ShowBackgroundOnTimer;

    public bool UseClockBackgroundGradient => !WindowedOperation && _optionsService.Options.ShowBackgroundOnClock;

    public bool WindowedOperation
    {
        get => _windowedOperation;
        set
        {
            if (_windowedOperation != value)
            {
                _windowedOperation = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(UseTimerBackgroundGradient));
                OnPropertyChanged(nameof(UseClockBackgroundGradient));
                OnPropertyChanged(nameof(BorderThickness));
                OnPropertyChanged(nameof(TimerBorderThickness));
                OnPropertyChanged(nameof(ClockIsFlat));
            }
        }
    }

    public bool SplitAndFullScreenModeIdentical()
    {
        return _optionsService.Options.AnalogueClockWidthPercent == 100;
    }

    private void OnShutDown(object recipient, ShutDownMessage obj)
    {
        ApplicationClosing = true;
    }

    private void OnShowTimeOfDayUnderTimerChanged(object recipient, ShowTimeOfDayUnderTimerChangedMessage message)
    {
        ShowTimeOfDayUnderTimer = _optionsService.Options.ShowTimeOfDayUnderTimer;
    }

    private void OnAnalogueClockWidthChanged(object recipient, AnalogueClockWidthChangedMessage obj)
    {
        AnalogueClockColumnWidthPercentage = _optionsService.Options.AnalogueClockWidthPercent;
    }

    private void OnDigitalClockFormatChanged(object recipient, ClockHourFormatChangedMessage obj)
    {
        OnPropertyChanged(nameof(DigitalTimeFormat24Hours));
        OnPropertyChanged(nameof(DigitalTimeFormatShowLeadingZero));
        OnPropertyChanged(nameof(DigitalTimeFormatAMPM));
        OnPropertyChanged(nameof(DigitalTimeShowSeconds));
    }

    private void OnTimerStopped(object recipient, TimerStopMessage obj)
    {
        IsRunning = false;
        DurationSector = null;
    }
    
    private static double CalcAngleFromTime(DateTime dt) =>
        (dt.Minute * 6.0) +
        (dt.Second * 0.1) +
        (dt.Millisecond * (0.1 / 1000));
    
    private void OnTimerStarted(object recipient, TimerStartMessage message)
    {
        TimeString = TimeFormatter.FormatTimerDisplayString(message.CountUp 
            ? 0
            : message.TargetSeconds);

        IsRunning = true;
        InitOverallDurationSector(0, message.TargetSeconds);
    }

    private void InitOverallDurationSector(int elapsedSecs, int remainingSecs)
    {
        if (DurationSector == null && 
            _optionsService.Options.ShowDurationSector && 
            remainingSecs < _secsPerHour)
        {
            // can't display duration sector effectively when >= 1 hr

            var now = _dateTimeService.Now();
            var startAngle = CalcAngleFromTime(now);

            var endTime = now.AddSeconds(remainingSecs);
            var endAngle = CalcAngleFromTime(endTime);

            DurationSector = new DurationSector
            {
                StartAngle = startAngle,
                EndAngle = endAngle,
                CurrentAngle = startAngle,
                IsOvertime = false,
                ShowElapsedSector = (elapsedSecs + remainingSecs) < _secsPerHour
            };
        }
    }

    private void OnMousePointerChanged(object recipient, MousePointerInTimerDisplayChangedMessage message)
    {
        OnPropertyChanged(nameof(MousePointer));
    }

    private void OnTimerChanged(object recipient, TimerChangedMessage message)
    {
        if (message.TimerIsRunning)
        {
            TextColor = GreenYellowRedSelector.GetBrushForTimeRemaining(message.RemainingSecs, message.ClosingSecs);
                
            TimeString = TimeFormatter.FormatTimerDisplayString(message.CountUp
                ? message.ElapsedSecs
                : message.RemainingSecs);

            // if duration of talk is greater than 1hr we only start showing the sector
            // when remaining time is less than 1 hr (for sake of clarity)...
            InitOverallDurationSector(message.ElapsedSecs, message.RemainingSecs);

            if (DurationSector != null)
            {
                var now = _dateTimeService.Now();
                var currentAngle = CalcAngleFromTime(now);

                // prevent gratuitous updates
                if (Math.Abs(currentAngle - DurationSector.CurrentAngle) > 0.15)
                {
                    var d = DurationSector.Clone();
                    d.CurrentAngle = currentAngle;
                    d.IsOvertime = message.RemainingSecs < 0;
                    d.ShowElapsedSector = (message.ElapsedSecs + message.RemainingSecs) <= _secsPerHour;

                    DurationSector = d;
                }
            }
        }
    }

    private void OnTimerFrameChanged(object recipient, TimerFrameChangedMessage msg)
    {
        OnPropertyChanged(nameof(BorderThickness));
        OnPropertyChanged(nameof(TimerBorderThickness));
        OnPropertyChanged(nameof(UseTimerBackgroundGradient));
        OnPropertyChanged(nameof(UseClockBackgroundGradient));
    }

    private void OnClockIsFlatChanged(object recipient, ClockIsFlatChangedMessage msg)
    {
        OnPropertyChanged(nameof(ClockIsFlat));
    }
}