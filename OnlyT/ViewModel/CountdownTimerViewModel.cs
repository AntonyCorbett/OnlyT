using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
    internal class CountdownTimerViewModel : ViewModelBase
    {
        private bool _applicationClosing;

        public CountdownTimerViewModel()
        {
            // subscriptions...
            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public bool ApplicationClosing => _applicationClosing;
        private void OnShutDown(ShutDownMessage obj)
        {
            _applicationClosing = true;
        }
    }
}
