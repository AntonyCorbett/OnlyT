namespace OnlyT.ViewModel.Messages
{
    using OnlyT.Models;

    /// <summary>
    /// When the timer monitor is changed in the Settings page
    /// </summary>
    internal class TimerMonitorChangedMessage
    {
        public TimerMonitorChangedMessage(MonitorChangeDescription change)
        {
            Change = change;
        }

        public MonitorChangeDescription Change { get; }
    }
}
