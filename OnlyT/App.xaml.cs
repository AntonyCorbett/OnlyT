using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using OnlyT.Common.Services.DateTime;
using OnlyT.Services.Bell;
using OnlyT.Services.CommandLine;
using OnlyT.Services.CountdownTimer;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Services.OutputDisplays;
using OnlyT.Services.Report;
using OnlyT.Services.Snackbar;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;
using OnlyT.WebServer;

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
        private Mutex? _appMutex;
        private static readonly Lazy<CommandLineService> CommandLineServiceInstance =
            new Lazy<CommandLineService>();

        protected override void OnExit(ExitEventArgs e)
        {
            _appMutex?.Dispose();
            Log.Logger.Information("==== Exit ====");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureServices();

            if (AnotherInstanceRunning())
            {
                Shutdown();
            }
            else
            {
                ConfigureLogger();
            }
        }

        private void ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<ITalkTimerService, TalkTimerService>();
            serviceCollection.AddSingleton<IMonitorsService, MonitorsService>();
            serviceCollection.AddSingleton<IOptionsService, OptionsService>();
            serviceCollection.AddSingleton<ITalkScheduleService, TalkScheduleService>();
            serviceCollection.AddSingleton<IBellService, BellService>();
            serviceCollection.AddSingleton<IAdaptiveTimerService, AdaptiveTimerService>();
            serviceCollection.AddSingleton<IHttpServer, HttpServer>();
            serviceCollection.AddSingleton<ICommandLineService, CommandLineService>();
            serviceCollection.AddSingleton<ICountdownTimerTriggerService, CountdownTimerTriggerService>();
            serviceCollection.AddSingleton<ISnackbarService, SnackbarService>();
            serviceCollection.AddSingleton<ILocalTimingDataStoreService, LocalTimingDataStoreService>();
            serviceCollection.AddSingleton(DateTimeServiceFactory);
            serviceCollection.AddSingleton<ILogLevelSwitchService, LogLevelSwitchService>();
            serviceCollection.AddSingleton<IQueryWeekendService, QueryWeekendService>();
            serviceCollection.AddSingleton<ITimerOutputDisplayService, TimerOutputDisplayService>();
            serviceCollection.AddSingleton<ICountdownOutputDisplayService, CountdownOutputDisplayService>();
            serviceCollection.AddSingleton(CommandLineServiceFactory);

            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddSingleton<OperatorPageViewModel>();
            serviceCollection.AddSingleton<SettingsPageViewModel>();
            serviceCollection.AddSingleton<TimerOutputWindowViewModel>();
            serviceCollection.AddSingleton<CountdownTimerViewModel>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(serviceProvider);
        }

        private CommandLineService CommandLineServiceFactory(IServiceProvider arg)
        {
            return CommandLineServiceInstance.Value;
        }

        private IDateTimeService DateTimeServiceFactory(IServiceProvider arg)
        {
            return new DateTimeService(CommandLineServiceInstance.Value.DateTimeOnLaunch);
        }

        private void ConfigureLogger()
        {
            string logsDirectory = FileUtils.GetLogFolder();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(LogLevelSwitchService.LevelSwitch)
                .WriteTo.File(Path.Combine(logsDirectory, "log-{Date}.txt"), retainedFileCountLimit: 28)
                .CreateLogger();

            Log.Logger.Information("==== Launched ====");
            Log.Logger.Information($"Version {VersionDetection.GetCurrentVersion()}");
        }

        private bool AnotherInstanceRunning()
        {
            var commandLineService = Ioc.Default.GetService<ICommandLineService>()!;
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
