namespace OnlyT.Report.Services
{
    using System;

    internal class DateTimeService : IDateTimeService
    {
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }
}
