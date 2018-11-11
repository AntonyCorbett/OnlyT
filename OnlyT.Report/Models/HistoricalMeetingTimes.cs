namespace OnlyT.Report.Models
{
    using System.Collections.Generic;

    public class HistoricalMeetingTimes
    {
        private readonly List<MeetingTimeSummary> _mtgTimeSummaries;

        public HistoricalMeetingTimes()
        {
            _mtgTimeSummaries = new List<MeetingTimeSummary>();
        }

        public IEnumerable<MeetingTimeSummary> Summaries => _mtgTimeSummaries;

        public int MeetingCount => _mtgTimeSummaries.Count;

        public void Add(MeetingTimeSummary summary)
        {
            _mtgTimeSummaries.Add(summary);
        }

        public void Sort()
        {
            _mtgTimeSummaries.Sort((x, y) => x.MeetingDate.CompareTo(y.MeetingDate));
        }
    }
}
