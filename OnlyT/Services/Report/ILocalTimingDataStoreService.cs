namespace OnlyT.Services.Report
{
    using System;
    using OnlyT.Report.Models;

    public interface ILocalTimingDataStoreService
    {
        void InsertPlannedMeetingEnd(DateTime plannedEnd);

        void InsertMeetingStart(DateTime value);

        void InsertActualMeetingEnd();

        void InsertTimerStart(
            string description, 
            bool isStudentTalk,
            TimeSpan plannedDuration,
            TimeSpan adaptedDuration);

        void InsertTimerStop();

        MeetingTimes GetCurrentMeetingTimes();

        bool ValidCurrentMeetingTimes();

        void PurgeCurrentMeetingTimes();

        void Save();

        void DeleteAllData();

        HistoricalMeetingTimes GetHistoricalMeetingTimes();
    }
}
