using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.Services.Bell
{
   public interface IBellService
   {
      void Play(int volumePercent);
      bool IsPlaying { get; }
   }
}
