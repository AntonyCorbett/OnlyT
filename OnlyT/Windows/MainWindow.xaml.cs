namespace OnlyT.Windows
{
    using System.Threading.Tasks;
    using System.Windows;
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Messaging;
    using GalaSoft.MvvmLight.Threading;
    using Services.Options;
    using Utils;
    using ViewModel;
    using ViewModel.Messages;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double MainWindowMinWidth = 296;
        private const double MainWindowMinHeight = 270;

        private const double OperatorWindowDefWidth = 395; 
        private const double OperatorWindowDefHeight = 350;

        private const double SettingsWindowDefWidth = 535;
        private const double SettingsWindowDefHeight = 525;

        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<BringMainWindowToFrontMessage>(this, BringToFront);
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);

            MinHeight = MainWindowMinHeight;
            MinWidth = MainWindowMinWidth;
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);

            AdjustMainWindowPositionAndSize();
        }

        private void OnNavigate(NavigateMessage message)
        {
            if (message.OriginalPageName.Equals(OperatorPageViewModel.PageName))
            {
                // store the size of the operator page...
                SaveOperatorWindowSize();
            }
            else if (message.OriginalPageName.Equals(SettingsPageViewModel.PageName))
            {
                // store the size of the settings page...
                SaveSettingsWindowSize();
            }

            if (message.TargetPageName.Equals(OperatorPageViewModel.PageName))
            {
                WindowState = WindowState.Normal;

                SetOperatorWindowSize();
            }
            else if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
            {
                var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
                var sz = optionsService.Options.SettingsPageSize;
                if (sz != default)
                {
                    Width = sz.Width;
                    Height = sz.Height;
                }
                else
                {
                    Width = SettingsWindowDefWidth;
                    Height = SettingsWindowDefHeight;
                }
            }
        }

        private void SetOperatorWindowSize()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            var sz = optionsService.Options.OperatorPageSize;
            if (sz != default)
            {
                Width = sz.Width;
                Height = sz.Height;
            }
            else
            {
                Width = OperatorWindowDefWidth;
                Height = OperatorWindowDefHeight;
            }
        }

        private void BringToFront(BringMainWindowToFrontMessage message)
        {
            BringMainWindowToFront();
        }

        private void BringMainWindowToFront()
        {
            Task.Delay(100).ContinueWith((t) =>
            {
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    Activate();
                });
            });
        }

        private void AdjustMainWindowPositionAndSize()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            if (!string.IsNullOrEmpty(optionsService.Options.AppWindowPlacement))
            {
                this.SetPlacement(
                    optionsService.Options.AppWindowPlacement, 
                    new Size(OperatorWindowDefWidth, OperatorWindowDefHeight));

                SetOperatorWindowSize();
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPos();
            
            var m = (MainViewModel)DataContext;

            if (m.CurrentPageName.Equals(OperatorPageViewModel.PageName))
            {
                SaveOperatorWindowSize();
            }
            else if (m.CurrentPageName.Equals(SettingsPageViewModel.PageName))
            {
                SaveSettingsWindowSize();
            }

            m.Closing(e);
        }

        private void SaveWindowPos()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            optionsService.Options.AppWindowPlacement = this.GetPlacement();
            optionsService.Save();
        }

        private void SaveSettingsWindowSize()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            optionsService.Options.SettingsPageSize = new Size(Width, Height);
            optionsService.Save();
        }

        private void SaveOperatorWindowSize()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            optionsService.Options.OperatorPageSize = new Size(Width, Height);
            optionsService.Save();
        }
    }
}
