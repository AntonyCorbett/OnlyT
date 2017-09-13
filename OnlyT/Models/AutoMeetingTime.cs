using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.Options;

namespace OnlyT.Models
{
   /// <summary>
   /// Used for items in the Settings page, "Meeting" combo
   /// </summary>
   public class AutoMeetingTime
   {
      public MidWeekOrWeekend Id { get; set; }
      public string Name { get; set; }
   }
}
