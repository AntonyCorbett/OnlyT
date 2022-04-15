using System;

namespace OnlyT.Common.Services.DateTime
{
    public class QueryWeekendService : IQueryWeekendService
    {
        public bool IsWeekend(System.DateTime theDate, bool weekendIncludesFriday)
        {
            return
                theDate.DayOfWeek == DayOfWeek.Saturday ||
                theDate.DayOfWeek == DayOfWeek.Sunday ||
                (weekendIncludesFriday && theDate.DayOfWeek == DayOfWeek.Friday);
        }
    }
}
