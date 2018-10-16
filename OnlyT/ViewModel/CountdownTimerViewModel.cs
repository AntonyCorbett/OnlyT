namespace OnlyT.ViewModel
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using Messages;

    public class CountdownTimerViewModel : ViewModelBase
    {
        public CountdownTimerViewModel()
        {
            // subscriptions...
            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }
        
        public bool ApplicationClosing { get; private set; }

        private void OnShutDown(ShutDownMessage obj)
        {
            ApplicationClosing = true;
        }
    }
}
