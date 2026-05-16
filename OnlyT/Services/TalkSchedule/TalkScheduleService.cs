using CommunityToolkit.Mvvm.Messaging;
using OnlyT.Services.CommandLine;
using Serilog;

namespace OnlyT.Services.TalkSchedule
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Models;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.Utils;
    using Options;
    using ViewModel.Messages;

    /// <summary>
    /// Service to handle the delivery of a talk schedule based on current "Operating mode"
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class TalkScheduleService : ITalkScheduleService
    {
        private static readonly DateTime January2020Change = new(2020, 1, 6);

        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICommandLineService _commandLineService;
        private readonly bool _isJanuary2020OrLater;

        // the "talk_schedule.xml" file may exist in MyDocs\OnlyT..
        private Lazy<IEnumerable<TalkScheduleItem>> _fileBasedSchedule = null!;
        private Lazy<IEnumerable<TalkScheduleItem>> _autoSchedule = null!;
        private Lazy<IEnumerable<TalkScheduleItem>> _manualSchedule = null!;

        public TalkScheduleService(
            IOptionsService optionsService,
            IDateTimeService dateTimeService,
            ICommandLineService commandLineService)
        {
            _optionsService = optionsService;
            _dateTimeService = dateTimeService;
            _commandLineService = commandLineService;

            _isJanuary2020OrLater = dateTimeService.Now().Date >= January2020Change;

            Reset();

            // subscriptions...
            WeakReferenceMessenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
        }

        public void Reset()
        {
            var feedUri = _commandLineService.FeedUri;

            _fileBasedSchedule = new Lazy<IEnumerable<TalkScheduleItem>>(() => TalkScheduleFileBased.Read(_optionsService.Options.AutoBell, GetScheduleFilePath()));
            _autoSchedule = new Lazy<IEnumerable<TalkScheduleItem>>(() => TalkScheduleAuto.Read(_optionsService, _dateTimeService, feedUri, _isJanuary2020OrLater));
            _manualSchedule = new Lazy<IEnumerable<TalkScheduleItem>>(() => TalkScheduleManual.Read(_optionsService));
        }

        public bool SuccessGettingAutoFeedForMidWeekMtg()
        {
            return TalkScheduleAuto.SuccessGettingAutoFeedForMidWeekMtg;
        }

        public void SetModifiedDuration(int talkId, TimeSpan? modifiedDuration)
        {
            var t = GetTalkScheduleItem(talkId);
            if (t != null)
            {
                t.ModifiedDuration = modifiedDuration;
            }
        }

        public IEnumerable<TalkScheduleItem> GetTalkScheduleItems()
        {
            if (_optionsService.Options.OperatingMode == OperatingMode.ScheduleFile)
            {
                var filePath = GetScheduleFilePath();
                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    Log.Logger.Warning("Schedule file not found: {FilePath}. Reverting to manual mode.", filePath);
                    WeakReferenceMessenger.Default.Send(new ScheduleFileNotFoundMessage());
                    return _manualSchedule.Value;
                }
            }

            return _optionsService.Options.OperatingMode switch
            {
                OperatingMode.ScheduleFile => _fileBasedSchedule.Value,
                OperatingMode.Automatic => _autoSchedule.Value,
                // ReSharper disable once RedundantCaseLabel
                OperatingMode.Manual => _manualSchedule.Value,
                _ => _manualSchedule.Value
            };
        }

        public TalkScheduleItem? GetTalkScheduleItem(int id)
        {
            return GetTalkScheduleItems().SingleOrDefault(n => n.Id == id);
        }

        public int GetNext(int currentTalkId)
        {
            var talks = GetTalkScheduleItems().ToArray();
            if (_optionsService.Options.OperatingMode == OperatingMode.Manual)
            {
                return talks.First().Id;
            }

            var foundCurrent = false;
            for (var n = 0; n < talks.Length; ++n)
            {
                var thisTalk = talks[n];
                if (thisTalk.Id.Equals(currentTalkId))
                {
                    foundCurrent = true;
                }

                if (n != talks.Length - 1 && foundCurrent && talks[n + 1].ActualDuration != TimeSpan.Zero)
                {
                    return talks[n + 1].Id;
                }
            }

            return 0;
        }

        private string? GetScheduleFilePath()
        {
            var fileName = _optionsService.Options.ScheduleFile;
            if (!string.IsNullOrEmpty(fileName))
            {
                return Path.Combine(FileUtils.GetSchedulesFolderPath(), fileName);
            }

            return null;
        }

        private void OnTimerStopped(object recipient, TimerStopMessage message)
        {
            var talk = GetTalkScheduleItem(message.TalkId);
            if (talk != null)
            {
                talk.CompletedTimeSecs = message.ElapsedSecs;
            }
        }
    }
}
