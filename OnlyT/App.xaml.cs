using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using OnlyT.EventTracking;
using OnlyT.AutoUpdates;
using OnlyT.Common.Services.DateTime;
using OnlyT.Services.Bell;
using OnlyT.Services.CommandLine;
using OnlyT.Services.CountdownTimer;
using OnlyT.Services.LogLevelSwitch;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Services.OutputDisplays;
using OnlyT.Services.OverrunNotificationService;
using OnlyT.Services.Reminders;
using OnlyT.Services.Report;
using OnlyT.Services.Snackbar;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;
using OnlyT.Utils;
using OnlyT.ViewModel;
using OnlyT.WebServer;
using Sentry;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace OnlyT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class App : Application
    {
        private readonly string _appString = "OnlyTMeetingTimer";
        private Mutex? _appMutex;
        private static readonly Lazy<CommandLineService> CommandLineServiceInstance = new();

        public App()
        {
            InitSentry(); // Sentry docs require it to be in the ctor rather than in OnStartup
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _appMutex?.Dispose();

            if (Log.IsEnabled(LogEventLevel.Information))
            {
                Log.Logger.Information("==== Exit ====");
            }
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

            Current.DispatcherUnhandledException += CurrentDispatcherUnhandledException;
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
            serviceCollection.AddSingleton<IReminderService, ReminderService>();
            serviceCollection.AddSingleton<IOverrunService, OverrunService>();
            serviceCollection.AddSingleton(CommandLineServiceFactory);

            serviceCollection.AddSingleton<MainViewModel>();
            serviceCollection.AddSingleton<OperatorPageViewModel>();
            serviceCollection.AddSingleton<SettingsPageViewModel>();
            serviceCollection.AddSingleton<TimerOutputWindowViewModel>();
            serviceCollection.AddSingleton<CountdownTimerViewModel>();
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(serviceProvider);

            // possibly override docs folder from commandline
            var commandLineService = serviceProvider.GetService<ICommandLineService>()!;
            FileUtils.OverrideDocumentFolder(commandLineService.OnlyTDocsFolder);
        }

        private CommandLineService CommandLineServiceFactory(IServiceProvider arg)
        {
            return CommandLineServiceInstance.Value;
        }

#pragma warning disable U2U1011 // Return types should be specific
        private IDateTimeService DateTimeServiceFactory(IServiceProvider arg)
#pragma warning restore U2U1011 // Return types should be specific
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

                if (Log.IsEnabled(LogEventLevel.Information))
                {
                    Log.Logger.Information("==== Launched ====");
                    Log.Logger.Information("Version {Version}", VersionDetection.GetCurrentVersion());
                }
            }
            catch (Exception)
            {
                // logging won't work but silently fails
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

        private static void InitSentry()
        {
            // https://soundbox.sentry.io/
            // https://docs.sentry.io/platforms/dotnet/guides/wpf/
            SentrySdk.Init(o =>
            {
                // Tells which project in Sentry to send events to:
                o.Dsn = "https://a507dcd971e89dc9b69a630090030ba7@o4509644339281920.ingest.de.sentry.io/4509753015926864";

#if DEBUG
                o.Debug = true;
#endif
                o.IsGlobalModeEnabled = true;

                // o.TracesSampleRate = 1.0; // Adjust for performance monitoring. 1.0 means 100% of messages are sent.
            });
        }
    }
}
