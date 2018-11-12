namespace OnlyT.Services.Report
{
    using System;
    using OnlyT.Report.Models;

    public interface ILocalTimingDataStoreService
    {
        DateTime LastTimerStop { get; }

        void InsertPlannedMeetingEnd(DateTime plannedEnd);

        void InsertSongSegment(DateTime startTime, string description, TimeSpan plannedDuration);

        void InsertMeetingStart(DateTime value);

        void InsertActualMeetingEnd(DateTime end);

        void InsertTimerStart(
            string description, 
            bool isSongSegment,
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
