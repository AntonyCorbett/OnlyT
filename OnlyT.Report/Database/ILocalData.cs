namespace OnlyT.Report.Database
{
    using System;
    using System.Collections.Generic;
    using OnlyT.Report.Models;

    internal interface ILocalData
    {
        void Save(MeetingTimes? mtgTimes);

        MeetingTimes GetMeetingTimes(Guid session);

        IReadOnlyCollection<MeetingTimes> GetMeetingTimes(DateTime theDate);

        IReadOnlyCollection<MeetingTimes> GetMeetingTimesRange(DateTime startDate, DateTime endDate);

        void DeleteAllMeetingTimesData();
    }
}
