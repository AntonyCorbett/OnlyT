using GalaSoft.MvvmLight.Messaging;
using OnlyT.ViewModel.Messages;

namespace OnlyT.Services.TalkSchedule
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using Options;

    /// <summary>
    /// Service to handle the delivery of a talk schedule based on current "Operating mode"
    /// </summary>
    public class TalkScheduleService : ITalkScheduleService
    {
        private readonly IOptionsService _optionsService;

        // the "talk_schedule.xml" file may exist in MyDocs\OnlyT..
        private Lazy<IEnumerable<TalkScheduleItem>> _fileBasedSchedule;
        private Lazy<IEnumerable<TalkScheduleItem>> _autoSchedule;
        private Lazy<IEnumerable<TalkScheduleItem>> _manualSchedule;

        public TalkScheduleService(IOptionsService optionsService)
        {
            _optionsService = optionsService;
            Messenger.Default.Register<TimerStopMessage>(this, OnTimerStopped);
            Reset();
        }

        public void Reset()
        {
            _fileBasedSchedule = new Lazy<IEnumerable<TalkScheduleItem>>(TalkScheduleFileBased.Read);
            _autoSchedule = new Lazy<IEnumerable<TalkScheduleItem>>(() => TalkScheduleAuto.Read(_optionsService));
            _manualSchedule = new Lazy<IEnumerable<TalkScheduleItem>>(() => TalkScheduleManual.Read(_optionsService));
        }

        public IEnumerable<TalkScheduleItem> GetTalkScheduleItems()
        {
            switch (_optionsService.Options.OperatingMode)
            {
                case OperatingMode.ScheduleFile:
                    return _fileBasedSchedule.Value;

                case OperatingMode.Automatic:
                    return _autoSchedule.Value;

                default:
                // ReSharper disable once RedundantCaseLabel
                case OperatingMode.Manual:
                    return _manualSchedule.Value;
            }
        }

        public TalkScheduleItem GetTalkScheduleItem(int id)
        {
            return GetTalkScheduleItems()?.SingleOrDefault(n => n.Id == id);
        }

        public int GetNext(int currentTalkId)
        {
            var talks = GetTalkScheduleItems()?.ToArray();
            if (talks != null)
            {
                if (_optionsService.Options.OperatingMode == OperatingMode.Manual)
                {
                    return talks.First().Id;
                }

                bool foundCurrent = false;
                for (int n = 0; n < talks.Length; ++n)
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
            }

            return 0;
        }

        private void OnTimerStopped(TimerStopMessage message)
        {
            var talk = GetTalkScheduleItem(message.TalkId);
            if (talk != null)
            {
                talk.CompletedTimeSecs = message.ElapsedSecs;
            }
        }
    }
}
