using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.Options;

namespace OnlyT.Services.Bell
{
   public class BellService : IBellService
   {
      private readonly TimerBell _bell;

      public BellService()
      {
         _bell = new TimerBell();
      }

      public void Play(int volumePercent)
      {
         Task.Run(() =>
         {
            _bell.Play(volumePercent);
         });
      }
   }
}
