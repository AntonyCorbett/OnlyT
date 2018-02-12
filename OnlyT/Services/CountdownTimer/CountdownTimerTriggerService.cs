using System;
using System.Collections.Generic;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;
using OnlyT.Services.Options;
using OnlyT.Services.Options.MeetingStartTimes;
using OnlyT.ViewModel.Messages;

namespace OnlyT.Services.CountdownTimer
{
    public sealed class CountdownTimerTriggerService : ICountdownTimerTriggerService
    {
        private const int CountdownDurationMins = 5;
        private List<CountdownTriggerPeriod> _triggerPeriods;
        private readonly object _locker = new object();

        public CountdownTimerTriggerService(IOptionsService options)
        {
            UpdateTriggerPeriods(options.Options.MeetingStartTimes.Times);
            
            // subscriptions...
            Messenger.Default.Register<MeetingStartTimesChangeMessage>(this, OnMeetingStartTimesChanged);
        }

        private void OnMeetingStartTimesChanged(MeetingStartTimesChangeMessage message)
        {
            UpdateTriggerPeriods(message?.Times);
        }

        private void UpdateTriggerPeriods(List<MeetingStartTime> times)
        {
            lock (_locker)
            {
                _triggerPeriods = null;
                if (times != null)
                {
                    CalculateTriggerPeriods(times);
                }
            }
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
                                time.StartTime.Hours * 60 + time.StartTime.Minutes - CountdownDurationMins)),
                        End = today.Add(new TimeSpan(time.StartTime.Hours, time.StartTime.Minutes, 0))
                    });
                }
            }

            _triggerPeriods = triggerPeriods;
        }

        public bool IsInCountdownPeriod(out int secondsOffset)
        {
            lock (_locker)
            {
                if (_triggerPeriods != null)
                {
                    DateTime now = DateTime.Now;

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
    }
}
