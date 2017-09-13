using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.Options;

namespace OnlyT.Models
{
   /// <summary>
   /// Used for items in the Settings page, "Operating mode" combo
   /// </summary>
   public class OperatingModeItem
   {
      public string Name { get; set; }
      public OperatingMode Mode { get; set; }
   }
}
