namespace OnlyT.Common.Services.DateTime
{
    using System;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class DateTimeService : IDateTimeService
    {
        private readonly DateTime _localDateTimeAtStart;
        private readonly DateTime _forcedDateTimeAtStart;
        private readonly bool _useForcedDateTime;

        public DateTimeService(DateTime forceDateTimeAtStart)
        {
            _forcedDateTimeAtStart = forceDateTimeAtStart;
            _localDateTimeAtStart = DateTime.Now;
            
            _useForcedDateTime = forceDateTimeAtStart != DateTime.MinValue;
        }

        public DateTime Now()
        {
            if (_useForcedDateTime)
            {
                var interval = DateTime.Now - _localDateTimeAtStart;
                return _forcedDateTimeAtStart + interval;
            }

            return DateTime.Now;
        }

        public DateTime UtcNow()
        {
            if (_useForcedDateTime)
            {
                return Now().ToUniversalTime();
            }

            return DateTime.UtcNow;
        }

        public DateTime Today()
        {
            if (_useForcedDateTime)
            {
                return Now().Date;
            }

            return DateTime.Today;
        }
    }
}
