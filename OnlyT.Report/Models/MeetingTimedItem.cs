namespace OnlyT.Report.Models
{
    using System;

    public class MeetingTimedItem
    {
        public string Description { get; set; }
        
        public TimeSpan Start { get; set; }

        public TimeSpan End { get; set; }

        public bool IsSongSegment { get; set; }

        public TimeSpan PlannedDuration { get; set; }

        public TimeSpan AdaptedDuration { get; set; }

        public bool IsStudentTalk { get; set; }
    }
}
