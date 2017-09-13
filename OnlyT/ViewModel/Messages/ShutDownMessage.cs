using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.ViewModel.Messages
{
   /// <summary>
   /// When the app is shutting down
   /// </summary>
   internal class ShutDownMessage
   {
      /// <summary>
      /// Name of the current page
      /// </summary>
      public string CurrentPageName { get; }

      public ShutDownMessage(string currentPageName)
      {
         CurrentPageName = currentPageName;
      }
   }
}
