using System.Collections.Generic;
using OnlyT.Services.Options.MeetingStartTimes;

namespace OnlyT.ViewModel.Messages
{
    /// <summary>
    /// When the meeting start times are changed.
    /// </summary>
    internal class MeetingStartTimesChangeMessage
    {
        public List<MeetingStartTime> Times { get; set; }
    }
}
