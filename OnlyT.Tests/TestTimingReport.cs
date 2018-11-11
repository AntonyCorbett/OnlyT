namespace OnlyT.Tests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OnlyT.Report.Services;
    using OnlyT.Services.Report;

    [TestClass]
    public class TestTimingReport
    {
        private const int TotalMtgLengthMins = 105;
        private const int SongAndPrayerMins = 10;

        private readonly Random _random = new Random();

        [TestMethod]
        public void TestWeekend()
        {
            var startOfMtg = DateTime.Today + TimeSpan.FromHours(10);
            var plannedEnd = startOfMtg.AddMinutes(TotalMtgLengthMins - SongAndPrayerMins);

            DateTimeServiceForTests dateTimeService = new DateTimeServiceForTests();
            dateTimeService.Set(startOfMtg);

            var service = new LocalTimingDataStoreService(null, dateTimeService);
            service.DeleteAllData();
            
            service.InsertMeetingStart(startOfMtg);
            service.InsertPlannedMeetingEnd(plannedEnd);

            dateTimeService.Add(TimeSpan.FromMinutes(1));

            // song and prayer
            InsertTimer(service, dateTimeService, "Introductory Segment", false, 5);

            // public talk...
            InsertTimer(service, dateTimeService, "Public Talk", false, 30);

            // song
            InsertTimer(service, dateTimeService, "Interim Segment", false, 5);

            // WT...
            InsertTimer(service, dateTimeService, "Watchtower Study", false, 60);

            InsertTimer(service, dateTimeService, "Concluding Segment", false, 5);

            service.InsertActualMeetingEnd();

            service.Save();

            var file = TimingReportGeneration.ExecuteAsync(service, null).Result;
            Assert.IsNotNull(file);
        }

        private void InsertTimer(
            LocalTimingDataStoreService service,
            DateTimeServiceForTests dateTimeService,
            string talkDescription, 
            bool isStudentTalk, 
            int targetMins)
        {
            service.InsertTimerStart(talkDescription, isStudentTalk, TimeSpan.FromMinutes(targetMins), TimeSpan.FromMinutes(targetMins));
            dateTimeService.Add(GetDuration(targetMins));
            service.InsertTimerStop();
        }

        private TimeSpan GetDuration(double targetMinutes)
        {
            int variationSecs = (int)((targetMinutes / 20.0) * 60);
            var secsToAdd = _random.Next(-variationSecs, variationSecs);

            var result = targetMinutes + (secsToAdd / 60.0);
            return TimeSpan.FromMinutes(result);
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
                _value = _value + timeSpan;
            }

            public DateTime Now()
            {
                if (_value == default(DateTime))
                {
                    throw new ArgumentException();
                }

                return _value;
            }
        }
    }
}
