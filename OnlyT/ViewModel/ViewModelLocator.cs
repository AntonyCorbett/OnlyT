using Microsoft.Toolkit.Mvvm.DependencyInjection;

namespace OnlyT.ViewModel
{
    public class ViewModelLocator
    {
        public MainViewModel Main => Ioc.Default.GetService<MainViewModel>()!;

        public OperatorPageViewModel Operator => Ioc.Default.GetService<OperatorPageViewModel>()!;

        public SettingsPageViewModel Settings => Ioc.Default.GetService<SettingsPageViewModel>()!;

        public TimerOutputWindowViewModel Output => Ioc.Default.GetService<TimerOutputWindowViewModel>()!;

        public CountdownTimerViewModel Countdown => Ioc.Default.GetService<CountdownTimerViewModel>()!;
    }
}