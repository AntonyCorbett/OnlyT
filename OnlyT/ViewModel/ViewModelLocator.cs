using OnlyT.Services.CommandLine;

namespace OnlyT.ViewModel
{
    using CommonServiceLocator;
    using GalaSoft.MvvmLight.Ioc;
    using Services.Bell;
    using Services.CountdownTimer;
    using Services.Monitors;
    using Services.Options;
    using Services.TalkSchedule;
    using Services.Timer;
    using WebServer;

    public class ViewModelLocator
    {
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

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<OperatorPageViewModel>();
            SimpleIoc.Default.Register<SettingsPageViewModel>();
        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();

        public OperatorPageViewModel Operator => ServiceLocator.Current.GetInstance<OperatorPageViewModel>();

        public SettingsPageViewModel Settings => ServiceLocator.Current.GetInstance<SettingsPageViewModel>();

        // ReSharper disable once UnusedMember.Global
        public static void Cleanup()
        {
            // Clear the ViewModels
        }
    }
}