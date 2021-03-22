using System.Threading.Tasks;

namespace OnlyT.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OnlyT.Common.Services.DateTime;
    using OnlyT.Report.Models;
    using OnlyT.Report.Pdf;
    using OnlyT.Services.Report;

    [TestClass]
    public class TestTimingReport
    {
        private const int TotalMtgLengthMins = 105;

        // 3 mins 20 secs for interim song
        private readonly TimeSpan _interimDuration = new(0, 3, 20);

        private readonly Random _random = new();

        private readonly bool _useRandomTimes = true;

        [TestMethod]
        public async Task TestReportGeneration()
        {
            var dateTimeService = new DateTimeServiceForTests();

            const int weekCount = 20;
            var dateOfFirstMeeting = GetNearestDayOnOrAfter(DateTime.Today.AddDays(-weekCount * 7), DayOfWeek.Sunday).Date;

            MeetingTimes? lastMtgTimes = null;
            for (var wk = 0; wk < weekCount; ++wk)
            {
                var dateOfWeekendMtg = dateOfFirstMeeting.AddDays(wk * 7);
                var dateOfMidweekMtg = dateOfWeekendMtg.AddDays(4);

                StoreWeekendData(wk, dateOfWeekendMtg, dateTimeService);
                lastMtgTimes = await StoreMidweekDataAsync(wk, weekCount, dateOfMidweekMtg, dateTimeService);
            }

            Assert.IsNotNull(lastMtgTimes);
            WriteReport(dateTimeService, new QueryWeekendService(), lastMtgTimes);
        }

        private static void WriteReport(
            DateTimeServiceForTests dateTimeService, 
            IQueryWeekendService queryWeekendService,
            MeetingTimes lastMtgTimes)
        {
            var service = new LocalTimingDataStoreService(null, dateTimeService);
            var historicalTimes = service.GetHistoricalMeetingTimes();

            var report = new PdfTimingReport(
                lastMtgTimes, 
                historicalTimes,
                queryWeekendService,
                false,
                Path.GetTempPath());

            report.Execute();
        }

        private void StoreWeekendData(
            int week,
            DateTime dateOfWeekendMtg, 
            DateTimeServiceForTests dateTimeService)
        {
            var startOfMtg = dateOfWeekendMtg + TimeSpan.FromHours(10);
            var plannedEnd = startOfMtg.AddMinutes(TotalMtgLengthMins);

            var service = new LocalTimingDataStoreService(null, dateTimeService);

            if (week == 0)
            {
                service.DeleteAllData();
            }

            dateTimeService.Set(startOfMtg);

            service.InsertMeetingStart(startOfMtg);
            service.InsertPlannedMeetingEnd(plannedEnd);

            dateTimeService.Add(TimeSpan.FromSeconds(1));

            // song and prayer
            InsertTimer(service, dateTimeService, "Introductory Segment", true, false, 5, false);

            // public talk...
            InsertTimer(service, dateTimeService, "Public Talk", false, false, 30);

            // song
            InsertTimer(service, dateTimeService, "Interim Segment", true, false, _interimDuration, false);

            // WT...
            InsertTimer(service, dateTimeService, "Watchtower Study", false, false, 60);

            InsertTimer(service, dateTimeService, "Concluding Segment", true, false, 5, false);

            service.InsertActualMeetingEnd(dateTimeService.Now());

            service.Save();
        }
        
        private async Task<MeetingTimes?> StoreMidweekDataAsync(
            int week,
            int weekCount,
            DateTime dateOfMidweekMtg,
            DateTimeServiceForTests dateTimeService)
        {
            var startOfMtg = dateOfMidweekMtg + TimeSpan.FromHours(19);
            var plannedEnd = startOfMtg.AddMinutes(TotalMtgLengthMins);

            var service = new LocalTimingDataStoreService(null, dateTimeService);

            dateTimeService.Set(startOfMtg);

            service.InsertMeetingStart(startOfMtg);
            service.InsertPlannedMeetingEnd(plannedEnd);
            
            InsertTimer(service, dateTimeService, "Introductory Segment", true, false, 5, false);

            InsertTimer(service, dateTimeService, "Opening Comments", false, false, 3);

            InsertTimer(service, dateTimeService, "Treasures", false, false, 10);

            InsertTimer(service, dateTimeService, "Digging for Spiritual Gems", false, false, 8);

            InsertTimer(service, dateTimeService, "Bible Reading", false, true, 4);
            dateTimeService.Add(GetCounselDuration());

            InsertTimer(service, dateTimeService, "Ministry Talk 1", false, true, 2);
            dateTimeService.Add(GetCounselDuration());

            InsertTimer(service, dateTimeService, "Ministry Talk 2", false, true, 4);
            dateTimeService.Add(GetCounselDuration());

            InsertTimer(service, dateTimeService, "Ministry Talk 3", false, true, 6);
            dateTimeService.Add(GetCounselDuration());

            InsertTimer(service, dateTimeService, "Interim Segment", true, false, _interimDuration, false);

            InsertTimer(service, dateTimeService, "Living Item 1", false, false, 15);

            InsertTimer(service, dateTimeService, "Congregation Bible Study", false, false, 30);

            InsertTimer(service, dateTimeService, "Review", false, false, 3);

            InsertTimer(service, dateTimeService, "Concluding Segment", true, false, 5, false);

            service.InsertActualMeetingEnd(dateTimeService.Now());

            service.Save();

            if (week == weekCount - 1)
            {
                var file = await TimingReportGeneration.ExecuteAsync(
                    service, 
                    dateTimeService, 
                    new QueryWeekendService(), 
                    false,
                    null);

                Assert.IsNotNull(file);
            }

            return service.MeetingTimes;
        }

        private TimeSpan GetCounselDuration()
        {
            if (_useRandomTimes)
            {
                return TimeSpan.FromSeconds(_random.Next(70, 90));
            }

            return TimeSpan.FromSeconds(80);
        }

        private void InsertTimer(
            LocalTimingDataStoreService service,
            DateTimeServiceForTests dateTimeService,
            string talkDescription, 
            bool isSongSegment,
            bool isStudentTalk, 
            TimeSpan target,
            bool addChangeoverTime = true)
        {
            service.InsertTimerStart(talkDescription, isSongSegment, isStudentTalk, target, target);
            dateTimeService.Add(GetDuration(target.TotalMinutes));
            service.InsertTimerStop();

            if (addChangeoverTime)
            {
                // add an amount for speaker swap
                AddSpeakerChangeoverTime(dateTimeService, isStudentTalk);
            }
        }

        private void InsertTimer(
            LocalTimingDataStoreService service,
            DateTimeServiceForTests dateTimeService,
            string talkDescription,
            bool isSongSegment,
            bool isStudentTalk,
            int targetMins,
            bool addChangeoverTime = true)
        {
            InsertTimer(service, dateTimeService, talkDescription, isSongSegment, isStudentTalk, TimeSpan.FromMinutes(targetMins), addChangeoverTime);
        }

        private void AddSpeakerChangeoverTime(DateTimeServiceForTests dateTimeService, bool isStudentTalk)
        {
            if (_useRandomTimes)
            {
                dateTimeService.Add(GetDuration(0.3));
            }
            else
            {
                if (!isStudentTalk)
                {
                    dateTimeService.Add(new TimeSpan(0, 0, 20));
                }
            }
        }

        private static DateTime GetNearestDayOnOrAfter(DateTime dt, DayOfWeek dayOfWeek)
        {
            return dt.AddDays(((int)dayOfWeek - (int)dt.DayOfWeek + 7) % 7);
        }

        private TimeSpan GetDuration(double targetMinutes)
        {
            if (_useRandomTimes)
            {
                int variationSecs = (int)((targetMinutes / 20.0) * 60);
                var secsToAdd = _random.Next(-variationSecs, variationSecs);

                var result = targetMinutes + (secsToAdd / 60.0);
                return TimeSpan.FromMinutes(result);
            }

            return TimeSpan.FromMinutes(targetMinutes);
        }

        private class DateTimeServiceForTests : IDateTimeService
        {
            private DateTime _value;
            
            public void Set(DateTime dt)
            {
                _value = dt;
            }

            public void Add(TimeSpan timeSpan)
            {
                _value += timeSpan;
            }

            public DateTime Now()
            {
                if (_value == default)
                {
                    throw new NotSupportedException("date was not set");
                }

                return _value;
            }

            public DateTime UtcNow()
            {
                return Now();
            }

            public DateTime Today()
            {
                return Now().Date;
            }
        }
    }
}
