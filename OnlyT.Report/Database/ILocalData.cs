namespace OnlyT.Report.Database
{
    using System;
    using System.Collections.Generic;
    using OnlyT.Report.Models;

    internal interface ILocalData
    {
        void Save(MeetingTimes mtgTimes);

        MeetingTimes GetMeetingTimes(Guid session);

        IEnumerable<MeetingTimes> GetMeetingTimes(DateTime theDate);

        IEnumerable<MeetingTimes> GetMeetingTimesRange(DateTime startDate, DateTime endDate);

        void DeleteAllMeetingTimesData();
    }
}
