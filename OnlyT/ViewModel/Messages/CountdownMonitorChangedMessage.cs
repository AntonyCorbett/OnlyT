using OnlyT.Models;

namespace OnlyT.ViewModel.Messages;

/// <summary>
/// When the countdown monitor is changed in the Settings page
/// </summary>
internal sealed class CountdownMonitorChangedMessage
{
    public CountdownMonitorChangedMessage(MonitorChangeDescription change)
    {
        Change = change;
    }

    public MonitorChangeDescription Change { get; }
}