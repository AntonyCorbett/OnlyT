using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.Services.Options
{
   public interface IOptionsService
   {
      Options Options { get; }
      void Save();

      bool IsTimerMonitorSpecified { get; }
   }
}
