namespace OnlyT.Windows
{
    using System.Windows;
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Messaging;
    using Services.Options;
    using Utils;
    using ViewModel;
    using ViewModel.Messages;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double MainWindowWidth = 395;
        private const double MainWindowHeight = 350;

        private const double SettingsWindowDefWidth = 535;
        private const double SettingsWindowDefHeight = 525;

        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<TimerMonitorChangedMessage>(this, BringToFront);
            Messenger.Default.Register<CountdownMonitorChangedMessage>(this, BringToFront);
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            AdjustMainWindowPositionAndSize();
        }

        private void OnNavigate(NavigateMessage message)
        {
            if (message.OriginalPageName.Equals(SettingsPageViewModel.PageName))
            {
                // store the size of the settings page...
                SaveSettingsWindowSize();
            }

            if (message.TargetPageName.Equals(OperatorPageViewModel.PageName))
            {
                // We don't allow the main window to be resized...
                ResizeMode = ResizeMode.CanMinimize;
                WindowState = WindowState.Normal;
                Width = MainWindowWidth;
                Height = MainWindowHeight;
            }
            else if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
            {
                // Settings window can be resized...
                ResizeMode = ResizeMode.CanResize;
                MinHeight = MainWindowHeight;
                MinWidth = MainWindowWidth;

                var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
                var sz = optionsService.Options.SettingsPageSize;
                if (sz != default(Size))
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

        private void BringToFront(TimerMonitorChangedMessage message)
        {
            Activate();
        }

        private void BringToFront(CountdownMonitorChangedMessage message)
        {
            Activate();
        }

        private void AdjustMainWindowPositionAndSize()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            if (!string.IsNullOrEmpty(optionsService.Options.AppWindowPlacement))
            {
                this.SetPlacement(optionsService.Options.AppWindowPlacement);
                this.SetPlacement(optionsService.Options.AppWindowPlacement, new Size(MainWindowWidth, MainWindowHeight));
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPos();
            
            var m = (MainViewModel)DataContext;

            if (m.CurrentPageName.Equals(SettingsPageViewModel.PageName))
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
        }
    }
}
