namespace OnlyT.Models
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    using System.Windows.Forms;

    /// <summary>
    /// Used for items in the Settings page, "Monitor" combo
    /// </summary>
    public class MonitorItem
    {
        public MonitorItem(Screen? monitor, string monitorName, string? monitorId, string friendlyName)
        {
            Monitor = monitor;
            MonitorName = monitorName;
            MonitorId = monitorId;
            FriendlyName = friendlyName;
        }

        public Screen? Monitor { get; }

        public string MonitorName { get; }

        public string? MonitorId { get; }

        public string FriendlyName { get; }
    }
}
