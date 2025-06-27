﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using OnlyT.Services.Options;
using OnlyT.Utils;
using OnlyT.ViewModel;
using OnlyT.ViewModel.Messages;
using MaterialDesignThemes.Wpf;

namespace OnlyT.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const double MainWindowDefaultWidth = 395;
    private const double MainWindowDefaultHeight = 350;

    private const double MainWindowMinNormalWidth = 250;
    private const double MainWindowMinNormalHeight = 200;
            
    private const double MainWindowMinWidthAbsolute = 143;
    private const double MainWindowMinHeightAbsolute = 118;

    private const double MainWindowDefaultShrunkWidth = 143;
    private const double MainWindowDefaultShrunkHeight = 118;

    private const double SettingsWindowDefWidth = 535;
    private const double SettingsWindowDefHeight = 525;

    private const double SettingsWindowMinWidth = 400;
    private const double SettingsWindowMinHeight = 360;

    private bool _isShrunk;

    public MainWindow()
    {
        InitializeComponent();

        Height = MainWindowDefaultHeight;
        Width = MainWindowDefaultWidth;

        WeakReferenceMessenger.Default.Register<BringMainWindowToFrontMessage>(this, BringToFront);
        WeakReferenceMessenger.Default.Register<BeforeNavigateMessage>(this, OnBeforeNavigate);
        WeakReferenceMessenger.Default.Register<NavigateMessage>(this, OnNavigate);
        WeakReferenceMessenger.Default.Register<ExpandFromShrinkMessage>(this, OnExpandFromShrink);
        WeakReferenceMessenger.Default.Register<DarkModeChangedMessage>(this, OnDarkModeChanged);

        MinHeight = MainWindowMinHeightAbsolute;
        MinWidth = MainWindowMinWidthAbsolute;

        SizeChanged += MainWindow_SizeChanged;
    }

    protected override void OnSourceInitialized(System.EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Trigger in OnSourceInitialized to ensure the window is available for the win32 API
        WeakReferenceMessenger.Default.Send(new DarkModeChangedMessage());

        var source = (HwndSource?)PresentationSource.FromVisual(this);
        source?.AddHook(HandleMessages);

        var optionsService = Ioc.Default.GetService<IOptionsService>()!;

        if (optionsService.Options.AppWindowUseShrunkPlacementAtStart)
        {
            AdjustMainWindowShrunkPositionAndSize();
        }
        else
        {
            AdjustMainWindowNormalPositionAndSize();
        }
    }

    private void OnBeforeNavigate(object recipient, BeforeNavigateMessage message)
    {
        SaveWindowPos();
        var m = (MainViewModel?)DataContext;
        if (m?.CurrentPage != null)
        {
            m.CurrentPage = null;

            // Force UI to update before proceeding
            Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);
        }
    }

    private void OnNavigate(object recipient, NavigateMessage message)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (message.TargetPageName.Equals(OperatorPageViewModel.PageName))
            {
                MinHeight = MainWindowMinHeightAbsolute;
                MinWidth = MainWindowMinWidthAbsolute;

                WindowState = WindowState.Normal;
                AdjustMainWindowNormalPositionAndSize();
            }
            else if (message.TargetPageName.Equals(SettingsPageViewModel.PageName))
            {
                WindowState = WindowState.Normal;
                AdjustMainWindowSettingsPositionAndSize();

                // restrict the min size of the window when on the Settings page
                MinWidth = SettingsWindowMinWidth;
                MinHeight = SettingsWindowMinHeight;
            }
        }), System.Windows.Threading.DispatcherPriority.Background);
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

    private void AdjustMainWindowNormalPositionAndSize()
    {
        var optionsService = Ioc.Default.GetService<IOptionsService>()!;
        if (!string.IsNullOrEmpty(optionsService.Options.AppWindowPlacementNormal))
        {
            this.SetPlacement(optionsService.Options.AppWindowPlacementNormal);
        }
        else
        {
            Width = MainWindowDefaultWidth;
            Height = MainWindowDefaultHeight;

            SaveWindowPos();
        }
    }

    private void AdjustMainWindowSettingsPositionAndSize()
    {
        var optionsService = Ioc.Default.GetService<IOptionsService>()!;
        if (!string.IsNullOrEmpty(optionsService.Options.AppWindowPlacementSettings))
        {
            this.SetPlacement(optionsService.Options.AppWindowPlacementSettings);
        }
        else
        {
            Width = SettingsWindowDefWidth;
            Height = SettingsWindowDefHeight;

            SaveWindowPos();
        }
    }

    private void AdjustMainWindowShrunkPositionAndSize()
    {
        var optionsService = Ioc.Default.GetService<IOptionsService>()!;
        if (!string.IsNullOrEmpty(optionsService.Options.AppWindowPlacementShrunk))
        {
            this.SetPlacement(optionsService.Options.AppWindowPlacementShrunk);
        }
        else
        {
            // default shrunk pos and size...

            Width = MainWindowDefaultShrunkWidth;
            Height = MainWindowDefaultShrunkHeight;

            SaveWindowPos();
        }
    }

    private void OnDarkModeChanged(object recipient, DarkModeChangedMessage message)
    {        
        var optionsService = Ioc.Default.GetService<IOptionsService>()!;
        var isDarkMode = optionsService.Options.DarkModeToggle;

        PaletteHelper paletteHelper = new();
        ITheme theme = paletteHelper.GetTheme();

        theme.SetBaseTheme(isDarkMode ? Theme.Dark : Theme.Light);
        paletteHelper.SetTheme(theme);

        // Update custom colours dictionary
        var app = (App)Application.Current;
        app.Resources.MergedDictionaries.RemoveAt(app.Resources.MergedDictionaries.Count - 1); // Remove old custom colours
        app.Resources.MergedDictionaries.Add(
            new ResourceDictionary { Source = new Uri(isDarkMode ? 
                "Themes/CustomColoursDark.xaml" : "Themes/CustomColours.xaml", UriKind.Relative) });

        var darkModeInt = isDarkMode ? 1 : 0;
        
        _ = NativeMethods.DwmSetWindowAttribute(
            new WindowInteropHelper(this).Handle,
            NativeMethods.DWMWA_USE_IMMERSIVE_DARK_MODE,
            ref darkModeInt,
            sizeof(int));
    }    

    private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        SaveWindowPos();

        var m = (MainViewModel?)DataContext;
        m?.Closing(e);
    }

    private void SaveWindowPos()
    {
        var placement = this.GetPlacement();

        if (string.IsNullOrEmpty(placement))
        {
            return;
        }

        var m = (MainViewModel?)DataContext;
        if (string.IsNullOrEmpty(m?.CurrentPageName))
        {
            return;
        }

        var optionsService = Ioc.Default.GetService<IOptionsService>()!;

        if (m.CurrentPageName.Equals(OperatorPageViewModel.PageName))
        {
            if (_isShrunk)
            {
                optionsService.Options.AppWindowUseShrunkPlacementAtStart = true;
                optionsService.Options.AppWindowPlacementShrunk = placement;
            }
            else
            {
                optionsService.Options.AppWindowUseShrunkPlacementAtStart = false;
                optionsService.Options.AppWindowPlacementNormal = placement;
            }
        }
        else if (m.CurrentPageName.Equals(SettingsPageViewModel.PageName))
        {
            optionsService.Options.AppWindowPlacementSettings = placement;
        }

        optionsService.Save();
    }

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _isShrunk = e.NewSize.Height < MainWindowMinNormalHeight || e.NewSize.Width < MainWindowMinNormalWidth;
        WeakReferenceMessenger.Default.Send(new MainWindowSizeChangedMessage(e.PreviousSize, e.NewSize, _isShrunk));

        // no caption bar when shrunk
        WindowStyle = _isShrunk ? WindowStyle.None : WindowStyle.SingleBorderWindow;
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
        SaveWindowPos();
        AdjustMainWindowNormalPositionAndSize();
    }

    private IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        // 0x0112 == WM_SYSCOMMAND, 'Window' command message.
        // 0xF020 == SC_MINIMIZE, command to minimize the window.
#pragma warning disable CA2020
        if (msg != 0x0112 || ((int) wParam & 0xFFF0) != 0xF020)
#pragma warning restore CA2020
        {
            return IntPtr.Zero;
        }

        // Handle minimize...

        var optionsService = Ioc.Default.GetService<IOptionsService>()!;
        if (!optionsService.Options.ShrinkOnMinimise)
        {
            handled = false;
        }
        else if (_isShrunk)
        {
            // can't minimize a shrunk window
            handled = true;
        }
        else
        {
            var m = (MainViewModel?) DataContext;

            if (m?.CurrentPageName?.Equals(SettingsPageViewModel.PageName) == true)
            {
                // we can minimise the window if we are on the Settings page
                handled = false;
            }
            else
            {
                // on the Operator page
                SaveWindowPos();
                AdjustMainWindowShrunkPositionAndSize();
                handled = true;
            }
        }

        return IntPtr.Zero;
    }
}