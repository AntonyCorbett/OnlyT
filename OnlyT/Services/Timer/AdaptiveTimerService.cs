using System;
using OnlyT.Services.Options;

namespace OnlyT.Services.Timer
{
   /// <summary>
   /// Used in Automatic mode to automatically adapt talk timings according to
   /// the start time of the meeting and the start time of the talk
   /// </summary>
   internal class AdaptiveTimerService : IAdaptiveTimerService
   {
      private DateTime _meetingStartTime;

      public AdaptiveTimerService()
      {
         
      }

      public void SetMeetingStart(IOptionsService optionsService, DateTime dt)
      {
         _meetingStartTime = dt;

         // store meeting start time in file systems in case
         // we have to restart OnlyT midway through a meeting...
         optionsService.Options.MeetingStart = dt;
         optionsService.Save();
      }

      


   }
}
