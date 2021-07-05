namespace OnlyT.Common.Services.DateTime
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class DateTimeService : IDateTimeService
    {
        private readonly DateTime _localDateTimeAtStart;
        private readonly DateTime? _forcedDateTimeAtStart;
        
        public DateTimeService(DateTime? forceDateTimeAtStart)
        {
            _forcedDateTimeAtStart = forceDateTimeAtStart;
            _localDateTimeAtStart = DateTime.Now;
        }

        public DateTime Now()
        {
            if (_forcedDateTimeAtStart != null)
            {
                var interval = DateTime.Now - _localDateTimeAtStart;
                return _forcedDateTimeAtStart.Value + interval;
            }

            return DateTime.Now;
        }

        public DateTime UtcNow()
        {
            if (_forcedDateTimeAtStart != null)
            {
                return Now().ToUniversalTime();
            }

            return DateTime.UtcNow;
        }

        public DateTime Today()
        {
            if (_forcedDateTimeAtStart != null)
            {
                return Now().Date;
            }

            return DateTime.Today;
        }
    }
}
