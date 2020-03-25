namespace OnlyT.ViewModel.Messages
{
    using OnlyT.Models;

    /// <summary>
    /// When the countdown monitor is changed in the Settings page
    /// </summary>
    internal class CountdownMonitorChangedMessage
    {
        public CountdownMonitorChangedMessage(MonitorChangeDescription change)
        {
            Change = change;
        }

        public MonitorChangeDescription Change { get; }
    }
}
