using System.Windows.Forms;

namespace OnlyT.Models
{
   /// <summary>
   /// Used for items in the Settings page, "Monitor" combo
   /// </summary>
   public class MonitorItem
   {
      public Screen Monitor { get; set; }
      public string MonitorName { get; set; }
      public string MonitorId { get; set; }
   }
}
