namespace OnlyT.Services.Report
{
    using System;
    using System.IO;
    using OnlyT.Report.Database;
    using OnlyT.Report.Models;
    using OnlyT.Report.Services;
    using OnlyT.Services.CommandLine;
    using OnlyT.Utils;
    using Serilog;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal class LocalTimingDataStoreService : ILocalTimingDataStoreService
    {
        private const int MeetingMinsOutOfRange = 20;

        private readonly ICommandLineService _commandLineService;
        private readonly IDateTimeService _dateTimeService;

        private LocalData _localData;
        private string _currentPartDescription;
        private bool _currentPartIsStudentTalk;
        private MeetingTimes _mtgTimes;
        private bool _initialised;
        
        public LocalTimingDataStoreService(
            ICommandLineService commandLineService,
            IDateTimeService dateTimeService)
        {
            _commandLineService = commandLineService;
            _dateTimeService = dateTimeService;
        }

        public MeetingTimes MeetingTimes => _mtgTimes;

        public DateTime LastTimerStop
        {
            get
            {
                EnsureInitialised();
                return _mtgTimes.LastTimerStop;
            }
        }

        public void DeleteAllData()
        {
            try
            {
                EnsureInitialised();
                _localData.DeleteAllMeetingTimesData();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not delete data");
            }
        }

        public void InsertSongSegment(DateTime startTime, string description, TimeSpan plannedDuration)
        {
            try
            {
                EnsureInitialised();
                _mtgTimes?.InsertSongSegment(startTime, description, plannedDuration);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not insert meeting planned end");
            }
        }

        public void InsertConcludingSongSegment(DateTime startTime, DateTime endTime, string description, TimeSpan plannedDuration)
        {
            try
            {
                EnsureInitialised();
                _mtgTimes?.InsertConcludingSongSegment(startTime, endTime, description, plannedDuration);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not insert meeting planned end");
            }
        }

        public void InsertPlannedMeetingEnd(DateTime plannedEnd)
        {
            try
            {
                EnsureInitialised();
                _mtgTimes?.InsertPlannedMeetingEnd(plannedEnd);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not insert meeting planned end");
            }
        }

        public void InsertMeetingStart(DateTime value)
        {
            try
            {
                EnsureInitialised();
                _mtgTimes?.InsertMeetingStart(value);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not insert meeting start");
            }
        }

        public void InsertActualMeetingEnd(DateTime end)
        {
            try
            {
                EnsureInitialised();
                _mtgTimes?.InsertActualMeetingEnd(end);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not insert meeting end");
            }
        }

        public void InsertTimerStart(
            string description, 
            bool isSongSegment, 
            bool isStudentTalk, 
            TimeSpan plannedDuration, 
            TimeSpan adaptedDuration)
        {
            try
            {
                EnsureInitialised();
                _currentPartDescription = description;
                _currentPartIsStudentTalk = isStudentTalk;

                _mtgTimes?.InsertTimerStart(
                    description, isSongSegment, isStudentTalk, plannedDuration, adaptedDuration);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not insert timer start");
            }
        }
        
        public void InsertTimerStop()
        {
            try
            {
                EnsureInitialised();
                _mtgTimes?.InsertTimerStop(_currentPartDescription, _currentPartIsStudentTalk);
                _currentPartDescription = null;
                _currentPartIsStudentTalk = false;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not insert timer stop");
            }
        }

        public MeetingTimes GetCurrentMeetingTimes()
        {
            try
            {
                EnsureInitialised();
                return _localData?.GetMeetingTimes(_mtgTimes.Session);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not retrieve meeting times data");
            }

            return null;
        }

        public bool ValidCurrentMeetingTimes()
        {
            EnsureInitialised();

            bool valid =
               _mtgTimes.MeetingStart != default(TimeSpan) &&
               _mtgTimes.MeetingActualEnd != default(TimeSpan) &&
               Math.Abs(_mtgTimes.GetMeetingOvertime().TotalMinutes) < MeetingMinsOutOfRange;

            if (!valid)
            {
                if (_mtgTimes.MeetingStart == default(TimeSpan))
                {
                    Log.Logger.Warning("Meeting Start not set");
                }

                if (_mtgTimes.MeetingActualEnd == default(TimeSpan))
                {
                    Log.Logger.Warning("Meeting End not set");
                }

                var minsOvertime = _mtgTimes.GetMeetingOvertime().TotalMinutes;
                double mins = Math.Abs(_mtgTimes.GetMeetingOvertime().TotalMinutes);

                if (mins >= MeetingMinsOutOfRange)
                {
                    var t = TimeSpan.FromMinutes(mins);
                    var overOrUnderStr = minsOvertime > 0 ? "overtime" : "undertime";
                    Log.Logger.Warning($"Meeting duration is out of range ({t:g} {overOrUnderStr})");
                }
            }

            return valid;
        }

        public void PurgeCurrentMeetingTimes()
        {
            EnsureInitialised();
            _mtgTimes.Purge();
        }

        public void Save()
        {
            try
            {
                EnsureInitialised();
                _localData.Save(_mtgTimes);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not save timing data");
            }
        }

        public HistoricalMeetingTimes GetHistoricalMeetingTimes()
        {
            try
            {
                EnsureInitialised();

                var now = DateTime.Now.Date;

                if (_dateTimeService != null)
                {
                    now = _dateTimeService.Now().Date;
                }

                return _localData?.GetHistoricalTimingData(now);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not retrieve historical meeting data");
            }

            return null;
        }

        private void EnsureInitialised()
        {
            if (!_initialised)
            {
                Init();
                _initialised = true;
            }
        }

        private void Init()
        {
            try
            {
                string folder = FileUtils.GetTimingReportsDatabaseFolder(_commandLineService?.OptionsIdentifier);
                string dbFilePath = Path.Combine(folder, "TimingData.db");

                _localData = new LocalData(dbFilePath);
                _mtgTimes = new MeetingTimes(_dateTimeService);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Could not initialise store");
            }
        }
    }
}
