using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.ViewModel.Messages
{
   /// <summary>
   /// When we navigate between pages (e.g. from Operator page to Settings page)
   /// </summary>
   internal class NavigateMessage
   {
      /// <summary>
      /// Name of the target page
      /// </summary>
      public string TargetPage { get; }

      /// <summary>
      /// Optional context-specific state
      /// </summary>
      public object State { get; }

      public NavigateMessage(string targetPage, object state)
      {
         TargetPage = targetPage;
         State = state;
      }
   }
}
