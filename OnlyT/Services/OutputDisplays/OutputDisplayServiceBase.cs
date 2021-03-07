namespace OnlyT.Services.OutputDisplays
{
    using System.Threading;
    using System.Windows;
    using OnlyT.Models;
    using OnlyT.Services.JwLibrary;
    using OnlyT.Services.Options;
    using OnlyT.Utils;
    using OnlyT.ViewModel.Messages;

    internal class OutputDisplayServiceBase
    {
        private readonly (int dpiX, int dpiY) _systemDpi;
        private readonly IOptionsService _optionsService;

        protected OutputDisplayServiceBase(IOptionsService optionsService)
        {
            _optionsService = optionsService;
            _systemDpi = WindowPlacement.GetDpiSettings();
        }

        protected void RelocateWindow(Window window, MonitorItem monitor)
        {
            if (monitor != null && window != null)
            {
                window.Hide();
                window.WindowState = WindowState.Normal;

                ShowWindowFullScreenOnTop(window, monitor);

                Messenger.Default.Send(new BringMainWindowToFrontMessage());
            }
        }

        protected void ShowWindowFullScreenOnTop(Window window, MonitorItem monitor)
        {
            if (monitor != null && window != null)
            {
                LocateWindowAtOrigin(window, monitor.Monitor);
                
                window.Topmost = true;
                window.Show();

                window.WindowState = WindowState.Maximized;
            }
        }

        protected void BringJwlToFront()
        {
            if (_optionsService.Options.JwLibraryCompatibilityMode)
            {
                JwLibHelper.BringToFront();
                Thread.Sleep(100);
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
}
