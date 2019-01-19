namespace OnlyT.Services.DateTime
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class DateTimeService : IDateTimeService
    {
        public System.DateTime Now()
        {
            return System.DateTime.Now;
        }

        public System.DateTime UtcNow()
        {
            return System.DateTime.UtcNow;
        }
    }
}
