using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace OnlyT.ViewModel
{
#pragma warning disable S1118 // Utility classes should not have public constructors
    public class ViewModelLocator
#pragma warning restore S1118 // Utility classes should not have public constructors
    {
        public static MainViewModel Main => Ioc.Default.GetService<MainViewModel>()!;

        public static OperatorPageViewModel Operator => Ioc.Default.GetService<OperatorPageViewModel>()!;

        public static SettingsPageViewModel Settings => Ioc.Default.GetService<SettingsPageViewModel>()!;

        public static TimerOutputWindowViewModel Output => Ioc.Default.GetService<TimerOutputWindowViewModel>()!;

        public static CountdownTimerViewModel Countdown => Ioc.Default.GetService<CountdownTimerViewModel>()!;
    }
}