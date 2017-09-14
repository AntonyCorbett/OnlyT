using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using OnlyT.Services.Bell;
using OnlyT.Services.Monitors;
using OnlyT.Services.Options;
using OnlyT.Services.TalkSchedule;
using OnlyT.Services.Timer;

namespace OnlyT.ViewModel
{
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

         SimpleIoc.Default.Register<MainViewModel>();
         SimpleIoc.Default.Register<OperatorPageViewModel>();
         SimpleIoc.Default.Register<SettingsPageViewModel>();
      }

      public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
      public OperatorPageViewModel Operator => ServiceLocator.Current.GetInstance<OperatorPageViewModel>();
      public SettingsPageViewModel Settings => ServiceLocator.Current.GetInstance<SettingsPageViewModel>();

      public static void Cleanup()
      {
         // Clear the ViewModels
      }
   }
}