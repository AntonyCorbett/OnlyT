namespace OnlyT.Common.Services.DateTime
{
    using System;
    using System.Diagnostics;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class DateTimeService : IDateTimeService
    {
        private readonly DateTime _baseTime;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public DateTimeService(DateTime? forceDateTimeAtStart)
        {
            _baseTime = forceDateTimeAtStart ?? DateTime.Now;
        }

        public DateTime Now() => _baseTime + _stopwatch.Elapsed;

        public DateTime UtcNow() => Now().ToUniversalTime();

        public DateTime Today() => Now().Date;
    }
}
