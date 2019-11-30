namespace OnlyT.Common.Services.DateTime
{
    using System;

    public interface IQueryWeekendService
    {
        bool IsWeekend(DateTime theDate, bool weekendIncludesFriday);
    }
}
