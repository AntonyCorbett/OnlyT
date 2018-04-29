namespace OnlyT.Services.CountdownTimer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Options;
    using Options.MeetingStartTimes;
    
    public sealed class CountdownTimerTriggerService : ICountdownTimerTriggerService
    {
        private const int CountdownDurationMins = 5;
        private readonly object _locker = new object();
        private readonly IOptionsService _optionsService;
        private List<CountdownTriggerPeriod> _triggerPeriods;

        public CountdownTimerTriggerService(IOptionsService optionsService)
        {
            _optionsService = optionsService;
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
                    var now = DateTime.Now;

                    var trigger = _triggerPeriods.FirstOrDefault(x => x.Start <= now && x.End > now);
                    if (trigger != null)
                    {
                        secondsOffset = (int)(now - trigger.Start).TotalSeconds;
                        return true;
                    }
                }
            }

            secondsOffset = 0;
            return false;
        }

        private void CalculateTriggerPeriods(IEnumerable<MeetingStartTime> meetingStartTimes)
        {
            var triggerPeriods = new List<CountdownTriggerPeriod>();

            DateTime today = DateTime.Today; // local time

            foreach (var time in meetingStartTimes)
            {
                if (time.DayOfWeek == null || time.DayOfWeek.Value == today.DayOfWeek)
                {
                    triggerPeriods.Add(new CountdownTriggerPeriod
                    {
                        Start = today.Add(
                            TimeSpan.FromMinutes(
                                (time.StartTime.Hours * 60) + time.StartTime.Minutes - CountdownDurationMins)),
                        End = today.Add(new TimeSpan(time.StartTime.Hours, time.StartTime.Minutes, 0))
                    });
                }
            }

            _triggerPeriods = triggerPeriods;
        }
    }
}
