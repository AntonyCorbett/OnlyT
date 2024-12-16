﻿using System.Threading;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Messaging;
using OnlyT.Models;
using OnlyT.Services.Options;
using OnlyT.Utils;
using OnlyT.ViewModel.Messages;

namespace OnlyT.Services.OutputDisplays;

internal class OutputDisplayServiceBase
{
    private readonly (int dpiX, int dpiY) _systemDpi;
    private readonly IOptionsService _optionsService;

    protected OutputDisplayServiceBase(IOptionsService optionsService)
    {
        _optionsService = optionsService;
        _systemDpi = WindowPlacement.GetDpiSettings();
    }

    protected void RelocateWindow(Window? window, MonitorItem? monitor)
    {
        if (monitor != null && window != null)
        {
            window.WindowState = WindowState.Normal;

            ShowWindowFullScreenOnTop(window, monitor);

            WeakReferenceMessenger.Default.Send(new BringMainWindowToFrontMessage());
        }
    }

    protected void ShowWindowFullScreenOnTop(Window? window, MonitorItem? monitor)
    {
        if (monitor?.Monitor != null && window != null)
        {
            LocateWindowAtOrigin(window, monitor.Monitor);
                
            window.Topmost = true;
            window.Show();

            window.WindowState = WindowState.Maximized;
        }
    }

    private void LocateWindowAtOrigin(Window window, Screen monitor)
    {
        var area = monitor.WorkingArea;

        var left = (area.Left * 96) / _systemDpi.dpiX;
        var top = (area.Top * 96) / _systemDpi.dpiY;
            
        window.Left = left;
        window.Top = top;
    }
}