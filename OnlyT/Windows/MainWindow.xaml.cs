using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Practices.ServiceLocation;
using OnlyT.Services.Options;
using OnlyT.Utils;
using OnlyT.ViewModel;
using OnlyT.ViewModel.Messages;

namespace OnlyT.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double MainWindowWidth = 395;
        private const double MainWindowHeight = 350;

        public MainWindow()
        {
            InitializeComponent();
            Messenger.Default.Register<TimerMonitorChangedMessage>(this, BringToFront);
            Messenger.Default.Register<NavigateMessage>(this, OnNavigate);
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
                Width = MainWindowWidth;
                Height = MainWindowHeight;
            }
            else if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
            {
                // Settings window can be resized...
                ResizeMode = ResizeMode.CanResize;
                var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
                var sz = optionsService.Options.SettingsPageSize;
                if (sz != default(Size))
                {
                    Width = sz.Width;
                    Height = sz.Height;
                }
            }
        }

        private void BringToFront(TimerMonitorChangedMessage message)
        {
            Activate();
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            AdjustMainWindowPositionAndSize();
        }

        private void AdjustMainWindowPositionAndSize()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            if (!string.IsNullOrEmpty(optionsService.Options.AppWindowPlacement))
            {
                this.SetPlacement(optionsService.Options.AppWindowPlacement, new Size(MainWindowWidth, MainWindowHeight));
            }
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveWindowPos();
            MainViewModel m = (MainViewModel)DataContext;
            m.Closing(sender, e);
        }

        private void SaveWindowPos()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            optionsService.Options.AppWindowPlacement = this.GetPlacement();
        }

        private void SaveSettingsWindowSize()
        {
            var optionsService = ServiceLocator.Current.GetInstance<IOptionsService>();
            optionsService.Options.SettingsPageSize = new Size(Width, Height);
        }
    }
}
