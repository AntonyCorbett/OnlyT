namespace OnlyT.Services.CountdownTimer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OnlyT.Common.Services.DateTime;
    using Options;
    using Options.MeetingStartTimes;

    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class CountdownTimerTriggerService : ICountdownTimerTriggerService
    {
        private readonly object _locker = new();
        private readonly IOptionsService _optionsService;
        private readonly IDateTimeService _dateTimeService;
        private List<CountdownTriggerPeriod>? _triggerPeriods;

        public CountdownTimerTriggerService(
            IOptionsService optionsService,
            IDateTimeService dateTimeService)
        {
            _optionsService = optionsService;
            _dateTimeService = dateTimeService;

            UpdateTriggerPeriods();
        }

        public void UpdateTriggerPeriods()
        {
            var times = _optionsService.Options.MeetingStartTimes.Times;

            lock (_locker)
            {
                _triggerPeriods = null;
                if (times != null)
                {
                    CalculateTriggerPeriods(times);
                }
            }
        }

        public bool IsInCountdownPeriod(out int secondsOffset)
        {
            lock (_locker)
            {
                if (_triggerPeriods != null)
                {
                    var now = _dateTimeService.Now();

                    var trigger = _triggerPeriods.FirstOrDefault(x => x.Start <= now && x.End > now);
                    if (trigger != null)
                    {
                        secondsOffset = (int)(now - trigger.Start).TotalSeconds;
                        return secondsOffset >= 5;
                    }
                }
            }

            secondsOffset = 0;
            return false;
        }

        private void CalculateTriggerPeriods(IEnumerable<MeetingStartTime> meetingStartTimes)
        {
            var triggerPeriods = new List<CountdownTriggerPeriod>();

            DateTime today = _dateTimeService.Today(); // local time
            int countdownDurationMins = _optionsService.Options.CountdownDurationMins;

            foreach (var time in meetingStartTimes)
            {
                if (time.DayOfWeek == null || time.DayOfWeek.Value == today.DayOfWeek)
                {
                    triggerPeriods.Add(new CountdownTriggerPeriod
                    {
                        Start = today.Add(
                            TimeSpan.FromMinutes(
                                (time.StartTime.Hours * 60) + time.StartTime.Minutes - countdownDurationMins)),
                        End = today.Add(new TimeSpan(time.StartTime.Hours, time.StartTime.Minutes, 0))
                    });
                }
            }

            _triggerPeriods = triggerPeriods;
        }
    }
}
