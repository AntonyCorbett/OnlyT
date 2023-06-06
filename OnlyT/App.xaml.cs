#if !DEBUG
#define USE_APP_CENTER
#endif

using System;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
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
using System.IO;
using System.Threading;
using System.Windows;
using CommunityToolkit.Mvvm.DependencyInjection;
using OnlyT.AutoUpdates;
using OnlyT.Services.LogLevelSwitch;
using OnlyT.ViewModel;
using Serilog;
using OnlyT.Utils;
using System.Diagnostics;
using OnlyT.EventTracking;

namespace OnlyT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly string _appString = "OnlyTMeetingTimer";
        private Mutex? _appMutex;
        private static readonly Lazy<CommandLineService> CommandLineServiceInstance = new();

        protected override void OnExit(ExitEventArgs e)
        {
            _appMutex?.Dispose();
            Log.Logger.Information("==== Exit ====");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ConfigureAppCenter();

            ConfigureServices();

            if (AnotherInstanceRunning())
            {
                Shutdown();
            }
            else
            {
                ConfigureLogger();
            }

            Current.DispatcherUnhandledException += CurrentDispatcherUnhandledException;
        }

        [Conditional("USE_APP_CENTER")]
        private static void ConfigureAppCenter()
        {
            AppCenterInit.Execute();
        }

        private void CurrentDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // unhandled exceptions thrown from UI thread
            EventTracker.Error(e.Exception, "Unhandled exception");

            e.Handled = true;
            Log.Logger.Fatal(e.Exception, "Unhandled exception");
            Current.Shutdown();
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

        private static void ConfigureLogger()
        {
            var logsDirectory = FileUtils.GetLogFolder();

            try
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.ControlledBy(LogLevelSwitchService.LevelSwitch)
                    .WriteTo.File(Path.Combine(logsDirectory, "log-.txt"), retainedFileCountLimit: 28, rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                Log.Logger.Information("==== Launched ====");
                Log.Logger.Information($"Version {VersionDetection.GetCurrentVersion()}");
            }
            catch (Exception ex)
            {
                // logging won't work but silently fails
                EventTracker.Error(ex, "Logging cannot be configured");

                // "no-op" logger
                Log.Logger = new LoggerConfiguration().CreateLogger();
            }
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
