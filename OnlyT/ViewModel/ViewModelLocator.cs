namespace OnlyT.ViewModel
{
    using System;
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using OnlyT.Services.Snackbar;
    using Services.Bell;
    using Services.CommandLine;
    using Services.CountdownTimer;
    using Services.Monitors;
    using Services.Options;
    using Services.TalkSchedule;
    using Services.Timer;
    using WebServer;

    public class ViewModelLocator
    {
        private static readonly Lazy<CommandLineService> CommandLineServiceInstance = 
            new Lazy<CommandLineService>();

        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<ITalkTimerService, TalkTimerService>();
            SimpleIoc.Default.Register<IMonitorsService, MonitorsService>();
            SimpleIoc.Default.Register<IOptionsService, OptionsService>();
            SimpleIoc.Default.Register<ITalkScheduleService, TalkScheduleService>();
            SimpleIoc.Default.Register<IBellService, BellService>();
            SimpleIoc.Default.Register<IAdaptiveTimerService, AdaptiveTimerService>();
            SimpleIoc.Default.Register<IHttpServer, HttpServer>();
            SimpleIoc.Default.Register<ICommandLineService, CommandLineService>();
            SimpleIoc.Default.Register<ICountdownTimerTriggerService, CountdownTimerTriggerService>();
            SimpleIoc.Default.Register<ISnackbarService, SnackbarService>();

            SimpleIoc.Default.Register(CommandLineServiceFactory);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<OperatorPageViewModel>();
            SimpleIoc.Default.Register<SettingsPageViewModel>();
            SimpleIoc.Default.Register<TimerOutputWindowViewModel>();
            SimpleIoc.Default.Register<CountdownTimerViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public OperatorPageViewModel Operator => ServiceLocator.Current.GetInstance<OperatorPageViewModel>();

        public SettingsPageViewModel Settings => ServiceLocator.Current.GetInstance<SettingsPageViewModel>();

        public TimerOutputWindowViewModel Output => ServiceLocator.Current.GetInstance<TimerOutputWindowViewModel>();

        public CountdownTimerViewModel Countdown => ServiceLocator.Current.GetInstance<CountdownTimerViewModel>();

        public static void Cleanup()
        {
            // Clear the ViewModels
        }

        internal static CommandLineService CommandLineServiceFactory()
        {
            return CommandLineServiceInstance.Value;
        }
    }
}