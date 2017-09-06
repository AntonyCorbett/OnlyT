using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using OnlyT.Timer;

namespace OnlyT.ViewModel
{
   public class ViewModelLocator
   {
      public ViewModelLocator()
      {
         ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
         
         SimpleIoc.Default.Register<ITalkTimerService, TalkTimerService>();
         SimpleIoc.Default.Register<MainViewModel>();
         SimpleIoc.Default.Register<OperatorPageViewModel>();
      }

      public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
      public OperatorPageViewModel Operator => ServiceLocator.Current.GetInstance<OperatorPageViewModel>();

      public static void Cleanup()
      {
         // TODO Clear the ViewModels
      }
   }
}