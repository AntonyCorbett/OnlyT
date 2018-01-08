using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Services.Options;
using OnlyT.ViewModel.Messages;

namespace OnlyT.ViewModel
{
    internal class CountdownTimerViewModel : ViewModelBase
    {
        private readonly IOptionsService _optionsService;
        private bool _applicationClosing;

        public CountdownTimerViewModel(IOptionsService optionsService)
        {
            _optionsService = optionsService;

            Messenger.Default.Register<ShutDownMessage>(this, OnShutDown);
        }

        public bool ApplicationClosing => _applicationClosing;
        private void OnShutDown(ShutDownMessage obj)
        {
            _applicationClosing = true;
        }
    }
}
