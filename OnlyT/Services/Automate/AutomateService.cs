namespace OnlyT.Services.Automate
{
    using System;
    using System.Diagnostics;
    using System.Windows.Threading;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.Models;
    using OnlyT.Services.Options;
    using OnlyT.Services.TalkSchedule;
    using OnlyT.Services.Timer;

    internal class AutomateService : IAutomateService
    {
        private const int TimerIntervalSeconds = 3;

        private readonly IOptionsService _optionsService;
        private readonly ITalkTimerService _timerService;
        private readonly ITalkScheduleService _scheduleService;
        private readonly IDateTimeService _dateTimeService;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly Random _random = new Random();

        private TimeSpan? _nextStartTime;
        private TimeSpan? _nextStopTime;

        public AutomateService(
            IOptionsService optionsService, 
            ITalkTimerService timerService,
            ITalkScheduleService scheduleService,
            IDateTimeService dateTimeService)
        {
            _optionsService = optionsService;
            _timerService = timerService;
            _scheduleService = scheduleService;
            _dateTimeService = dateTimeService;
        }
        
        public void Execute()
        {
            if (_optionsService.Options.OperatingMode != OperatingMode.Automatic)
            {
                throw new Exception("Must be in automatic mode!");
            }

            _timer.Interval = TimeSpan.FromSeconds(TimerIntervalSeconds);
            _timer.Tick += TimerTick;
            _timer.Start();
        }

        private void TimerTick(object sender, System.EventArgs e)
        {
            var now = _dateTimeService.Now();

            if (!_stopwatch.IsRunning)
            {
                // not yet started the meeting...
                if (now.Minute % 15 == 0)
                {
                    // start on the nearest quarter hour...
                    _stopwatch.Start();
                    _nextStartTime = now.TimeOfDay.Add(TimeSpan.FromMinutes(5));
                }

                return;
            }

            var status = _timerService.GetStatus();
            CheckIsCurrentTalk(status);

            if (_nextStartTime != null && now.TimeOfDay > _nextStartTime)
            {
                CheckNotRunning(status);
            
                if (status.TalkId != null)
                {
                    _timerService.StartTalkTimerFromApi(status.TalkId.Value);
                }

                _nextStartTime = null;
                _nextStopTime = now.TimeOfDay.Add(TimeSpan.FromSeconds(status.TargetSeconds));

                return;
            }

            if (_nextStopTime != null && now.TimeOfDay > _nextStopTime)
            {
                CheckIsRunning(status);
                
                if (status.TalkId != null)
                {
                    _timerService.StopTalkTimerFromApi(status.TalkId.Value);
                }

                _nextStopTime = null;
                _nextStartTime = CalculateNextStartTime(status, now);

                if (_nextStartTime == null)
                {
                    // all done
                    _stopwatch.Stop();
                    _timer.Stop();
                }
            }
        }

        private TimeSpan? CalculateNextStartTime(TimerStatus status, DateTime now)
        {
            if (_optionsService.Options.MidWeekOrWeekend == MidWeekOrWeekend.MidWeek)
            {
                return CalculateNextStartTimeMidWeek(status, now);
            }

            return CalculateNextStartTimeWeekend(status, now);
        }

        private TimeSpan? CalculateNextStartTimeWeekend(TimerStatus status, DateTime now)
        {
            if (status.TalkId == null)
            {
                return null;
            }

            _timerService.GetStatus();
            var talkType = (TalkTypesAutoMode)status.TalkId;

            switch (talkType)
            {
                case TalkTypesAutoMode.PublicTalk:
                    return now.TimeOfDay.Add(GetAboutXMinutes(4)); // for interim segment
            }

            return null;
        }

        private TimeSpan GetAboutXMinutes(int x)
        {
            return GetAboutXSeconds(x * 60);
        }

        private TimeSpan GetAboutXSeconds(int x)
        {
            var tolerance = x / 10;
            return TimeSpan.FromSeconds(_random.Next(x - tolerance, x + tolerance));
        }

        private TimeSpan? CalculateNextStartTimeMidWeek(TimerStatus status, DateTime now)
        {
            if (status.TalkId == null)
            {
                return null;
            }

            _timerService.GetStatus();
            var talkType = (TalkTypesAutoMode)status.TalkId;

            switch (talkType)
            {
                case TalkTypesAutoMode.OpeningComments:
                case TalkTypesAutoMode.TreasuresTalk:
                case TalkTypesAutoMode.DiggingTalk:
                    return now.TimeOfDay.Add(GetAboutXSeconds(20));

                case TalkTypesAutoMode.Reading:
                    return now.TimeOfDay.Add(GetAboutXSeconds(80));

                case TalkTypesAutoMode.MinistryItem1:
                case TalkTypesAutoMode.MinistryItem2:
                case TalkTypesAutoMode.MinistryItem3:
                case TalkTypesAutoMode.MinistryItem4:
                    var lastMinistryItemBeforeSong = GetLastMinistryItem();
                    var item = _scheduleService.GetTalkScheduleItem((int)talkType);
                    CheckItem(item);
                    return item == lastMinistryItemBeforeSong 
                        ? now.TimeOfDay.Add(GetAboutXMinutes(4)) 
                        : now.TimeOfDay.Add(GetAboutXSeconds(item.IsStudentTalk ? 80 : 20));

                case TalkTypesAutoMode.LivingPart1:
                case TalkTypesAutoMode.LivingPart2:
                case TalkTypesAutoMode.CongBibleStudy:
                    return now.TimeOfDay.Add(GetAboutXSeconds(20));

                default:
                    return null;
            }
        }

        private TalkScheduleItem GetLastMinistryItem()
        {
            var item = _scheduleService.GetTalkScheduleItem((int)TalkTypesAutoMode.MinistryItem2);
            if (item.OriginalDuration == TimeSpan.Zero)
            {
                return _scheduleService.GetTalkScheduleItem((int)TalkTypesAutoMode.MinistryItem1);
            }

            item = _scheduleService.GetTalkScheduleItem((int)TalkTypesAutoMode.MinistryItem3);
            if (item.OriginalDuration == TimeSpan.Zero)
            {
                return _scheduleService.GetTalkScheduleItem((int)TalkTypesAutoMode.MinistryItem2);
            }

            item = _scheduleService.GetTalkScheduleItem((int)TalkTypesAutoMode.MinistryItem4);
            if (item == null || item.OriginalDuration == TimeSpan.Zero)
            {
                return _scheduleService.GetTalkScheduleItem((int)TalkTypesAutoMode.MinistryItem3);
            }

            return item;
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void CheckItem(TalkScheduleItem item)
        {
            if (item == null)
            {
                throw new Exception("Could not find item in talk schedule!");
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void CheckIsCurrentTalk(TimerStatus status)
        {
            if (status.TalkId == null)
            {
                throw new Exception("Unexpected talk status!");
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void CheckNotRunning(TimerStatus status)
        {
            if (status.IsRunning)
            {
                throw new Exception("Unexpected timer status!");
            }
        }

        // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
        private void CheckIsRunning(TimerStatus status)
        {
            if (!status.IsRunning)
            {
                throw new Exception("Unexpected timer status!");
            }
        }
    }
}
