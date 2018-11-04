namespace OnlyT.ViewModel
{
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
        }

        public bool ApplicationClosing { get; private set; }

        public int BorderThickness => _optionsService.Options.CountdownFrame ? 3 : 0;

        public int BackgroundOpacity => _optionsService.Options.CountdownFrame ? 100 : 0;

        private void OnShutDown(ShutDownMessage obj)
        {
            ApplicationClosing = true;
        }

        private void OnFrameChanged(CountdownFrameChangedMessage msg)
        {
            RaisePropertyChanged(nameof(BorderThickness));
            RaisePropertyChanged(nameof(BackgroundOpacity));
        }
    }
}
