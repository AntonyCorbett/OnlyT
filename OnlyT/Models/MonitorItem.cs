using System.Text;
using System.Windows.Forms;
using OnlyT.Properties;

namespace OnlyT.Models;

/// <summary>
/// Used for items in the Settings page, "Monitor" combo
/// </summary>
public class MonitorItem
{
    public MonitorItem(Screen? monitor, string monitorName, string? monitorId, string friendlyName, bool primary)
    {
        Monitor = monitor;
        MonitorName = monitorName;
        MonitorId = monitorId;
        FriendlyName = friendlyName;
        Primary = primary;
    }

    public Screen? Monitor { get; }

    public string MonitorName { get; }

    public string? MonitorId { get; }

    public string FriendlyName { get; }

    public bool Primary { get; }

    public string NameForDisplayInUI
    {
        get
        {
            var sb = new StringBuilder(FriendlyName);
            if (Primary)
            {
                sb.Append(" (");
                sb.Append(Resources.PRIMARY_MONITOR);
                sb.Append(')');
            }

            return sb.ToString();
        }
    }
}