namespace OnlyT.Report.Services
{
    using System;

    public class DateTimeService : IDateTimeService
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }
}
