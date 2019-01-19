namespace OnlyT.Report.Services
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }

        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
