namespace OnlyT.Services.OutputDisplays
{
    public interface ICountdownOutputDisplayService
    {
        bool IsCountdownDone { get; }

        bool IsCountingDown { get; }

        bool IsWindowAvailable();

        void RelocateWindow();

        bool IsWindowVisible();

        void Activate();
        
        bool OpenWindowInMonitor();

        bool OpenWindowWindowed();

        void Start(int offsetSeconds);

        void Stop(bool manuallyStopped);

        void Hide();

        void Close();

        void SaveWindowedPos();
    }
}
