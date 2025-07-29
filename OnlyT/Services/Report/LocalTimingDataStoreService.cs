using System;
using System.IO;
using OnlyT.Common.Services.DateTime;
using OnlyT.EventTracking;
using OnlyT.Report.Database;
using OnlyT.Report.Models;
using OnlyT.Services.CommandLine;
using OnlyT.Utils;
using Serilog;

namespace OnlyT.Services.Report;

// ReSharper disable once ClassNeverInstantiated.Global
internal sealed class LocalTimingDataStoreService : ILocalTimingDataStoreService
{
    private const int MeetingMinsOutOfRange = 20;

    private readonly ICommandLineService? _commandLineService;
    private readonly IDateTimeService _dateTimeService;

    private LocalData? _localData;
    private string? _currentPartDescription;
    private bool _currentPartIsStudentTalk;
    private MeetingTimes? _mtgTimes;
    private bool _initialised;
        
    public LocalTimingDataStoreService(
        ICommandLineService? commandLineService,
        IDateTimeService dateTimeService)
    {
        _commandLineService = commandLineService;
        _dateTimeService = dateTimeService;
    }

    public MeetingTimes? MeetingTimes => _mtgTimes;

    public DateTime LastTimerStop
    {
        get
        {
            EnsureInitialised();
            return _mtgTimes?.LastTimerStop ?? DateTime.MinValue;
        }
    }

    public void DeleteAllData()
    {
        try
        {
            EnsureInitialised();
            _localData?.DeleteAllMeetingTimesData();
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not delete data";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
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
            const string errMsg = "Could not insert song segment";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
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
            const string errMsg = "Could not insert concluding song segment";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
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
            const string errMsg = "Could not insert meeting planned end";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
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
            const string errMsg = "Could not insert meeting start";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
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
            const string errMsg = "Could not insert meeting end";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
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
            const string errMsg = "Could not insert timer start";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
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
            const string errMsg = "Could not insert timer stop";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    public MeetingTimes? GetCurrentMeetingTimes()
    {
        try
        {
            EnsureInitialised();

            return _mtgTimes == null 
                ? null 
                : _localData?.GetMeetingTimes(_mtgTimes.Session);
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not retrieve meeting times data";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }

        return null;
    }

    public bool ValidCurrentMeetingTimes()
    {
        EnsureInitialised();

        if (_mtgTimes == null)
        {
            return false;
        }

        var valid =
            _mtgTimes.MeetingStart != default &&
            _mtgTimes.MeetingActualEnd != default &&
            Math.Abs(_mtgTimes.GetMeetingOvertime().TotalMinutes) < MeetingMinsOutOfRange;

        if (!valid)
        {
            if (_mtgTimes.MeetingStart == default)
            {
                Log.Logger.Warning("Meeting Start not set");
            }

            if (_mtgTimes.MeetingActualEnd == default)
            {
                Log.Logger.Warning("Meeting End not set");
            }
            
            var mins = Math.Abs(_mtgTimes.GetMeetingOvertime().TotalMinutes);

            if (mins >= MeetingMinsOutOfRange)
            {
                var minsOvertime = _mtgTimes.GetMeetingOvertime().TotalMinutes;

                var t = TimeSpan.FromMinutes(mins);
                var overOrUnderStr = minsOvertime > 0 ? "overtime" : "undertime";
                Log.Logger.Warning("Meeting duration is out of range ({Duration} {Status})", t, overOrUnderStr);
            }
        }

        return valid;
    }

    public void PurgeCurrentMeetingTimes()
    {
        EnsureInitialised();
        _mtgTimes?.Purge();
    }

    public void Save()
    {
        try
        {
            EnsureInitialised();
            _localData?.Save(_mtgTimes);
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not save timing data";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }

    public HistoricalMeetingTimes? GetHistoricalMeetingTimes()
    {
        try
        {
            EnsureInitialised();

            var now = _dateTimeService.Now().Date;

            return _localData?.GetHistoricalTimingData(now);
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not retrieve historical meeting data";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
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
            var folder = FileUtils.GetTimingReportsDatabaseFolder(_commandLineService?.OptionsIdentifier);
            var dbFilePath = Path.Combine(folder, "TimingDataV2.db");

            _localData = new LocalData(dbFilePath);
            _mtgTimes = new MeetingTimes(_dateTimeService);
        }
        catch (Exception ex)
        {
            const string errMsg = "Could not initialise store";
            EventTracker.Error(ex, errMsg);

            Log.Logger.Error(ex, errMsg);
        }
    }
}