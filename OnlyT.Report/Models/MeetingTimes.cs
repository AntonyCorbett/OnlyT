namespace OnlyT.Report.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OnlyT.Common.Services.DateTime;

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

        public DateTime LastTimerStop { get; private set; }
        
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

        public void InsertSongSegment(
            DateTime startTime, string description, TimeSpan duration)
        {
            InsertSongTime(startTime, description, duration, true);
            InsertTimerStop(description, false);
        }

        public void InsertConcludingSongSegment(
            DateTime startTime, DateTime endTime, string description, TimeSpan duration)
        {
            InsertSongTime(startTime, description, duration, true);
            InsertTimerStopSpecifyingTime(endTime, description, false);
        }

        public void InsertMeetingStart(DateTime value)
        {
            InitMtgDate();
            MeetingStart = RoundTimeSpanToSecond(value.TimeOfDay);
        }

        public void InsertActualMeetingEnd(DateTime end)
        {
            InitMtgDate();
            MeetingActualEnd = RoundTimeSpanToSecond(end.TimeOfDay);
        }

        public void InsertTimerStart(
            string partDescription, 
            bool isSongSegment,
            bool isStudentTalk, 
            TimeSpan plannedDuration, 
            TimeSpan adaptedDuration)
        {
            InitMtgDate();

            Items.Add(new MeetingTimedItem
            {
                Description = partDescription,
                IsSongSegment = isSongSegment,
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

                LastTimerStop = MeetingDate + item.End;
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

        private void InsertSongTime(
            DateTime start,
            string partDescription,
            TimeSpan duration,
            bool isSongSegment)
        {
            InitMtgDate();

            Items.Add(new MeetingTimedItem
            {
                Description = partDescription,
                IsSongSegment = isSongSegment,
                Start = RoundTimeSpanToSecond(start.TimeOfDay),
                PlannedDuration = RoundTimeSpanToSecond(duration),
                AdaptedDuration = RoundTimeSpanToSecond(duration)
            });
        }

        private void InsertTimerStopSpecifyingTime(DateTime endTime, string partDescription, bool isStudentTalk)
        {
            InitMtgDate();

            var item = Items.FirstOrDefault(
                x => x.Description.Equals(partDescription) &&
                     x.IsStudentTalk == isStudentTalk &&
                     x.End.Equals(default(TimeSpan)));

            if (item != null)
            {
                item.End = RoundTimeSpanToSecond(endTime.TimeOfDay);

                LastTimerStop = MeetingDate + item.End;
            }
        }
    }
}
