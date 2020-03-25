namespace OnlyT.Services.OutputDisplays
{
    public interface ITimerOutputDisplayService
    {
        bool IsWindowAvailable();

        bool IsWindowVisible();

        void ShowWindow();

        void RelocateWindow();

        void HideWindow();

        void OpenWindowInMonitor();

        void OpenWindowWindowed();

        void Close();

        void SaveWindowedPos();
    }
}
