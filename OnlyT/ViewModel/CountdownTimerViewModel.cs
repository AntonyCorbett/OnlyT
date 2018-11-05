namespace OnlyT.ViewModel
{
    using System.Windows;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using Messages;
    using OnlyT.Services.Options;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class CountdownTimerViewModel : ViewModelBase
    {
        private readonly IOptionsService _optionsService;

        public CountdownTimerViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;

            // subscriptions...
            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
            Messenger.Default.Register<CountdownFrameChangedMessage>(this, OnFrameChanged);
            Messenger.Default.Register<CountdownZoomOrPositionChangedMessage>(this, OnZoomOrPositionChanged);
        }

        public bool ApplicationClosing { get; private set; }

        public int BorderThickness => _optionsService.Options.CountdownFrame ? 3 : 0;

        public int BackgroundOpacity => _optionsService.Options.CountdownFrame ? 100 : 0;

        public double CountdownScale => _optionsService.Options.CountdownZoomPercent / 100.0;

        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                switch (_optionsService.Options.CountdownScreenLocation)
                {
                    case ScreenLocation.Left:
                    case ScreenLocation.BottomLeft:
                    case ScreenLocation.TopLeft:
                        return HorizontalAlignment.Left;

                    case ScreenLocation.Right:
                    case ScreenLocation.BottomRight:
                    case ScreenLocation.TopRight:
                        return HorizontalAlignment.Right;

                    default:
                        return HorizontalAlignment.Center;
                }
            }
        }

        public VerticalAlignment VerticalAlignment
        {
            get
            {
                switch (_optionsService.Options.CountdownScreenLocation)
                {
                    case ScreenLocation.Top:
                    case ScreenLocation.TopLeft:
                    case ScreenLocation.TopRight:
                        return VerticalAlignment.Top;

                    case ScreenLocation.Bottom:
                    case ScreenLocation.BottomLeft:
                    case ScreenLocation.BottomRight:
                        return VerticalAlignment.Bottom;

                    default:
                        return VerticalAlignment.Center;
                }
            }
        }

        private void OnShutDown(ShutDownMessage obj)
        {
            ApplicationClosing = true;
        }

        private void OnFrameChanged(CountdownFrameChangedMessage msg)
        {
            RaisePropertyChanged(nameof(BorderThickness));
            RaisePropertyChanged(nameof(BackgroundOpacity));
        }

        private void OnZoomOrPositionChanged(CountdownZoomOrPositionChangedMessage obj)
        {
            RaisePropertyChanged(nameof(CountdownScale));
            RaisePropertyChanged(nameof(HorizontalAlignment));
            RaisePropertyChanged(nameof(VerticalAlignment));
        }
    }
}
