using System;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Messaging;

namespace OnlyT.Windows
{
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using Services.Options;
    using Utils;
    using ViewModel;
    using ViewModel.Messages;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const double MainWindowDefaultWidth = 395;
        private const double MainWindowDefaultHeight = 350;

        private const double MainWindowMinNormalWidth = 250;
        private const double MainWindowMinNormalHeight = 200;
            
        private const double MainWindowMinWidthAbsolute = 100;
        private const double MainWindowMinHeightAbsolute = 85;

        private const double OperatorWindowDefWidth = 395;
        private const double OperatorWindowDefHeight = 350;

        private const double SettingsWindowDefWidth = 535;
        private const double SettingsWindowDefHeight = 525;

        public MainWindow()
        {
            InitializeComponent();

            Height = MainWindowDefaultHeight;
            Width = MainWindowDefaultWidth;

            WeakReferenceMessenger.Default.Register<BringMainWindowToFrontMessage>(this, BringToFront);
            WeakReferenceMessenger.Default.Register<NavigateMessage>(this, OnNavigate);
            WeakReferenceMessenger.Default.Register<ExpandFromShrinkMessage>(this, OnExpandFromShrink);

            MinHeight = MainWindowMinHeightAbsolute;
            MinWidth = MainWindowMinWidthAbsolute;

            SizeChanged += MainWindow_SizeChanged;
        }

        protected override void OnSourceInitialized(System.EventArgs e)
        {
            base.OnSourceInitialized(e);

            AdjustMainWindowPositionAndSize();
        }

        private void OnNavigate(object recipient, NavigateMessage message)
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
                var optionsService = Ioc.Default.GetService<IOptionsService>()!;
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
            var optionsService = Ioc.Default.GetService<IOptionsService>()!;
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

        private void BringToFront(object recipient, BringMainWindowToFrontMessage message)
        {
            BringMainWindowToFront();
        }

        private void BringMainWindowToFront()
        {
            Task.Delay(100).ContinueWith((_) =>
                Application.Current.Dispatcher.BeginInvoke(new Action(() => Activate())));
        }

        private void AdjustMainWindowPositionAndSize()
        {
            var optionsService = Ioc.Default.GetService<IOptionsService>()!;
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

            var m = (MainViewModel?)DataContext;

            if (m?.CurrentPageName != null)
            {
                if (m.CurrentPageName.Equals(OperatorPageViewModel.PageName))
                {
                    SaveOperatorWindowSize();
                }
                else if (m.CurrentPageName.Equals(SettingsPageViewModel.PageName))
                {
                    SaveSettingsWindowSize();
                }
            }

            m?.Closing(e);
        }

        private void SaveWindowPos()
        {
            var optionsService = Ioc.Default.GetService<IOptionsService>()!;
            optionsService.Options.AppWindowPlacement = this.GetPlacement();
            optionsService.Save();
        }

        private void SaveSettingsWindowSize()
        {
            var optionsService = Ioc.Default.GetService<IOptionsService>()!;
            optionsService.Options.SettingsPageSize = new Size(Width, Height);
            optionsService.Save();
        }

        private void SaveOperatorWindowSize()
        {
            var optionsService = Ioc.Default.GetService<IOptionsService>()!;
            optionsService.Options.OperatorPageSize = new Size(Width, Height);
            optionsService.Save();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var isShrunk = e.NewSize.Height < MainWindowMinNormalHeight || e.NewSize.Width < MainWindowMinNormalWidth;
            WeakReferenceMessenger.Default.Send(new MainWindowSizeChangedMessage(e.PreviousSize, e.NewSize, isShrunk));

            // no caption bar when shrunk
            WindowStyle = isShrunk ? WindowStyle.None : WindowStyle.SingleBorderWindow;
        }

        private void WindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            // allow drag when no title bar is shown
            if (e.ChangedButton == MouseButton.Left && WindowStyle == WindowStyle.None)
            {
                DragMove();
            }
        }

        private void OnExpandFromShrink(object recipient, ExpandFromShrinkMessage message)
        {
            Width = MainWindowDefaultWidth;
            Height = MainWindowDefaultHeight;
        }
    }
}
