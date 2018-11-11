namespace OnlyT.Report.Models
{
    using System;

    public class MeetingTimeSummary
    {
        public DateTime MeetingDate { get; set; }

        public TimeSpan Overtime { get; set; }
    }
}
