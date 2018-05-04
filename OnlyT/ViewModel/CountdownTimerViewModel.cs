namespace OnlyT.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using Messages;

    internal class CountdownTimerViewModel : ViewModelBase
    {
        private bool _applicationClosing;

        public CountdownTimerViewModel()
        {
            // subscriptions...
            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        // ReSharper disable once UnusedMember.Global
        public bool ApplicationClosing => _applicationClosing;

        private void OnShutDown(ShutDownMessage obj)
        {
            _applicationClosing = true;
        }
    }
}
