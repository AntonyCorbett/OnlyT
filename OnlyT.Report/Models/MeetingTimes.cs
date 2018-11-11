namespace OnlyT.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OnlyT.Report.Services;

    public class MeetingTimes
    {
        private readonly IDateTimeService _dateTimeService;

        public MeetingTimes(IDateTimeService dateTimeService)
            : this()
        {
            _dateTimeService = dateTimeService;
            Session = Guid.NewGuid();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public MeetingTimes()
        {
            Items = new List<MeetingTimedItem>();
        }

        public int MeetingTimesId { get; set; }

        public Guid Session { get; set; }

        public DateTime MeetingDate { get; set; }

        public TimeSpan MeetingStart { get; set; }

        public TimeSpan MeetingActualEnd { get; set; }

        public TimeSpan MeetingPlannedEnd { get; set; }

        public List<MeetingTimedItem> Items { get; set; }
        
        public void InsertPlannedMeetingEnd(DateTime plannedEnd)
        {
            MeetingPlannedEnd = RoundTimeSpanToSecond(plannedEnd.TimeOfDay);
        }

        public void InsertMeetingStart(DateTime value)
        {
            InitMtgDate();
            MeetingStart = RoundTimeSpanToSecond(value.TimeOfDay);
        }

        public void InsertActualMeetingEnd()
        {
            InitMtgDate();
            MeetingActualEnd = RoundTimeSpanToSecond(Now().TimeOfDay);
        }

        public void InsertTimerStart(
            string partDescription, bool isStudentTalk, TimeSpan plannedDuration, TimeSpan adaptedDuration)
        {
            InitMtgDate();

            Items.Add(new MeetingTimedItem
            {
                Description = partDescription,
                IsStudentTalk = isStudentTalk,
                Start = RoundTimeSpanToSecond(Now().TimeOfDay),
                PlannedDuration = RoundTimeSpanToSecond(plannedDuration),
                AdaptedDuration = RoundTimeSpanToSecond(adaptedDuration)
            });
        }

        public void InsertTimerStop(string partDescription, bool isStudentTalk)
        {
            InitMtgDate();

            var item = Items.FirstOrDefault(
               x => x.Description.Equals(partDescription) &&
                    x.IsStudentTalk == isStudentTalk &&
                    x.End.Equals(default(TimeSpan)));

            if (item != null)
            {
                item.End = RoundTimeSpanToSecond(Now().TimeOfDay);
            }
        }

        public TimeSpan GetMeetingOvertime()
        {
            return MeetingActualEnd - MeetingPlannedEnd;
        }

        public void Purge()
        {
            MeetingDate = default(DateTime);
            InitMtgDate();
            MeetingStart = default(TimeSpan);
            MeetingActualEnd = default(TimeSpan);
            MeetingPlannedEnd = default(TimeSpan);
            Items.Clear();
        }

        private static DateTime RoundDateTimeToSecond(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
        }

        private static TimeSpan RoundTimeSpanToSecond(TimeSpan ts)
        {
            return TimeSpan.FromSeconds((int)Math.Round(ts.TotalSeconds));
        }

        private void InitMtgDate()
        {
            if (MeetingDate == default(DateTime))
            {
                MeetingDate = Now().Date;
            }
        }

        private DateTime Now()
        {
            return _dateTimeService?.Now() ?? DateTime.Now;
        }
    }
}
