using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace OnlyT.ViewModel
{
#pragma warning disable S1118 // Utility classes should not have public constructors
#pragma warning disable RCS1102 // Make class static.
    public class ViewModelLocator
#pragma warning restore RCS1102 // Make class static.
#pragma warning restore S1118 // Utility classes should not have public constructors
    {
        public static MainViewModel Main => Ioc.Default.GetService<MainViewModel>()!;

        public static OperatorPageViewModel Operator => Ioc.Default.GetService<OperatorPageViewModel>()!;

        public static SettingsPageViewModel Settings => Ioc.Default.GetService<SettingsPageViewModel>()!;

        public static TimerOutputWindowViewModel Output => Ioc.Default.GetService<TimerOutputWindowViewModel>()!;

        public static CountdownTimerViewModel Countdown => Ioc.Default.GetService<CountdownTimerViewModel>()!;
    }
}