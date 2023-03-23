using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using OnlyT.ViewModel.Messages;
using OnlyT.CountdownTimer;
using OnlyT.Services.Options;

namespace OnlyT.ViewModel
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class CountdownTimerViewModel : ObservableObject
    {
        private readonly IOptionsService _optionsService;
        private bool _windowedOperation;

        public CountdownTimerViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;

            // subscriptions...
            WeakReferenceMessenger.Default.Register<CountdownFrameChangedMessage>(this, OnFrameChanged);
            WeakReferenceMessenger.Default.Register<CountdownZoomOrPositionChangedMessage>(this, OnZoomOrPositionChanged);
            WeakReferenceMessenger.Default.Register<CountdownElementsChangedMessage>(this, OnElementsChanged);
            WeakReferenceMessenger.Default.Register<CountdownWindowTransparencyChangedMessage>(this, OnWindowTransparencyChanged);
            WeakReferenceMessenger.Default.Register<MousePointerInTimerDisplayChangedMessage>(this, OnMousePointerChanged);
        }
        
        public int BorderThickness => !WindowedOperation && _optionsService.Options.CountdownFrame ? 3 : 0;

        public int BackgroundOpacity => !WindowedOperation && _optionsService.Options.CountdownFrame ? 100 : 0;

        public double CountdownScale => WindowedOperation ? 1 : _optionsService.Options.CountdownZoomPercent / 100.0;

        public ElementsToShow ElementsToShow => _optionsService.Options.CountdownElementsToShow;

        public bool IsWindowTransparent => !WindowedOperation && _optionsService.Options.IsCountdownWindowTransparent;

        public int CountdownDurationMins => _optionsService.Options.CountdownDurationMins;

        public Cursor MousePointer =>
            _optionsService.Options.ShowMousePointerInTimerDisplay
                ? Cursors.Arrow
                : Cursors.None;

        public bool WindowedOperation
        {
            get => _windowedOperation;
            set
            {
                if (_windowedOperation != value)
                {
                    _windowedOperation = value;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(BorderThickness));
                    OnPropertyChanged(nameof(BackgroundOpacity));
                    OnPropertyChanged(nameof(CountdownScale));
                    OnPropertyChanged(nameof(IsWindowTransparent));
                }
            }
        }

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return _optionsService.Options.CountdownScreenLocation switch
                {
                    ScreenLocation.Left => HorizontalAlignment.Left,
                    ScreenLocation.BottomLeft => HorizontalAlignment.Left,
                    ScreenLocation.TopLeft => HorizontalAlignment.Left,
                    ScreenLocation.Right => HorizontalAlignment.Right,
                    ScreenLocation.BottomRight => HorizontalAlignment.Right,
                    ScreenLocation.TopRight => HorizontalAlignment.Right,
                    _ => HorizontalAlignment.Center
                };
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return _optionsService.Options.CountdownScreenLocation switch
                {
                    ScreenLocation.Top or 
                        ScreenLocation.TopLeft or 
                        ScreenLocation.TopRight => VerticalAlignment.Top,

                    ScreenLocation.Bottom or 
                        ScreenLocation.BottomLeft or 
                        ScreenLocation.BottomRight => VerticalAlignment.Bottom,

                    _ => VerticalAlignment.Center,
                };
            }
        }

        private void OnFrameChanged(object recipient, CountdownFrameChangedMessage msg)
        {
            OnPropertyChanged(nameof(BorderThickness));
            OnPropertyChanged(nameof(BackgroundOpacity));
        }

        private void OnWindowTransparencyChanged(object recipient, CountdownWindowTransparencyChangedMessage msg)
        {
            OnPropertyChanged(nameof(IsWindowTransparent));
        }

        private void OnMousePointerChanged(object recipient, MousePointerInTimerDisplayChangedMessage message)
        {
            OnPropertyChanged(nameof(MousePointer));
        }

        private void OnZoomOrPositionChanged(object recipient, CountdownZoomOrPositionChangedMessage obj)
        {
            OnPropertyChanged(nameof(CountdownScale));
            OnPropertyChanged(nameof(HorizontalAlignment));
            OnPropertyChanged(nameof(VerticalAlignment));
        }

        private void OnElementsChanged(object recipient, CountdownElementsChangedMessage obj)
        {
            OnPropertyChanged(nameof(ElementsToShow));
        }
    }
}
