namespace OnlyT.Report.Services
{
    using System;

    public interface IDateTimeService
    {
        DateTime Now();

        DateTime UtcNow();
    }
}
