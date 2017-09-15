using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlyT.Services.Options;

namespace OnlyT.Tests.Mocks
{
   public static class MockOptions
   {
      public static Options Create()
      {
         var result = new Options();
         return result;
      }

   }
}
