namespace OnlyT.Report.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OnlyT.Report.Models;

    public class LocalData : ILocalData
    {
        private const string CollectionNameMtgTimes = "meeting_times";
        private const int HistoricalMonths = 6;

        private readonly string _localDbFilePath;

        public LocalData(string localDbFilePath)
        {
            _localDbFilePath = localDbFilePath;
        }

        public void Save(MeetingTimes mtgTimes)
        {
            using (var ctx = new LocalDatabaseContext(_localDbFilePath))
            {
                var timings = ctx.Db.GetCollection<MeetingTimes>(CollectionNameMtgTimes);
                timings.Insert(mtgTimes);
            }
        }

        public MeetingTimes GetMeetingTimes(Guid session)
        {
            using (var ctx = new LocalDatabaseContext(_localDbFilePath))
            {
                var timings = ctx.Db.GetCollection<MeetingTimes>(CollectionNameMtgTimes);
                timings.EnsureIndex(x => x.Session);
                return timings.FindOne(x => x.Session.Equals(session));
            }
        }

        public IReadOnlyCollection<MeetingTimes> GetMeetingTimes(DateTime theDate)
        {
            using (var ctx = new LocalDatabaseContext(_localDbFilePath))
            {
                var timings = ctx.Db.GetCollection<MeetingTimes>(CollectionNameMtgTimes);
                timings.EnsureIndex(x => x.MeetingDate);

                var minDate = theDate.Date;
                var maxDate = theDate.Date.AddDays(1);
                return timings.Find(
                    x => x.MeetingDate >= minDate && x.MeetingDate < maxDate)
                    .OrderBy(x => x.MeetingTimesId).ToArray();
            }
        }

        public IReadOnlyCollection<MeetingTimes> GetMeetingTimesRange(DateTime startDate, DateTime endDate)
        {
            using (var ctx = new LocalDatabaseContext(_localDbFilePath))
            {
                var timings = ctx.Db.GetCollection<MeetingTimes>(CollectionNameMtgTimes);
                timings.EnsureIndex(x => x.MeetingDate);

                var minDate = startDate.Date;
                var maxDate = endDate.Date.AddDays(1);

                return timings.Find(
                    x => x.MeetingDate >= minDate && x.MeetingDate < maxDate)
                    .OrderBy(x => x.MeetingTimesId).ToArray();
            }
        }

        public HistoricalMeetingTimes? GetHistoricalTimingData(DateTime dt)
        {
            HistoricalMeetingTimes? result = null;

            var startDate = dt.AddMonths(-HistoricalMonths);
            var times = GetMeetingTimesRange(startDate, dt);

            foreach (var t in times)
            {
                if (t.MeetingPlannedEnd != default(TimeSpan) && t.MeetingActualEnd != default(TimeSpan))
                {
                    result ??= new HistoricalMeetingTimes();

                    var summary = new MeetingTimeSummary
                    {
                        MeetingDate = t.MeetingDate,
                        Overtime = t.GetMeetingOvertime()
                    };

                    result.Add(summary);
                }
            }

            result?.Sort();

            return result;
        }

        public void DeleteAllMeetingTimesData()
        {
            using (var ctx = new LocalDatabaseContext(_localDbFilePath))
            {
                ctx.Db.DropCollection(CollectionNameMtgTimes);
            }
        }
    }
}
