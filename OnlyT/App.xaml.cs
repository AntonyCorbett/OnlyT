namespace OnlyT
{
    using System.IO;
    using System.Threading;
    using System.Windows;
    using OnlyT.AutoUpdates;
    using OnlyT.Services.LogLevelSwitch;
    using OnlyT.ViewModel;
    using Serilog;
    using Utils;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string _appString = "OnlyTMeetingTimer";
        private Mutex _appMutex;

        public App()
        {
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _appMutex?.Dispose();
            Log.Logger.Information("==== Exit ====");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (AnotherInstanceRunning())
            {
                Shutdown();
            }
            else
            {
                ConfigureLogger();
            }
        }

        private void ConfigureLogger()
        {
            string logsDirectory = FileUtils.GetLogFolder();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LogLevelSwitchService.LevelSwitch)
                .WriteTo.File(Path.Combine(logsDirectory, "log-{Date}.txt"), retainedFileCountLimit: 28)

#if DEBUG
                .WriteTo.Debug()
#endif
                .CreateLogger();

            Log.Logger.Information("==== Launched ====");
            Log.Logger.Information($"Version {VersionDetection.GetCurrentVersion()}");
        }

        private bool AnotherInstanceRunning()
        {
            var commandLineService = ViewModelLocator.CommandLineServiceFactory();
            if (commandLineService.IgnoreMutex)
            {
                // if the "nomutex" option is specified then
                // it is possible to run multiple instances
                // (the user is responsible for ensuring that
                // settings are not shared).
                return false;
            }

            _appMutex = new Mutex(true, _appString, out var newInstance);
            return !newInstance;
        }
    }
}
