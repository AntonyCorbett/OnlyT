using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.ViewModel.Messages
{
   internal class OvertimeMessage
   {
      public string TalkName { get; }
      public bool UseBellForTalk { get; set; }
      public int TargetDurationSecs { get; set; }
   
      public OvertimeMessage(string talkName, bool useBellForTalk, int targetDurationSecs)
      {
         TalkName = talkName;
         UseBellForTalk = useBellForTalk;
         TargetDurationSecs = targetDurationSecs;
      }
   }
}
