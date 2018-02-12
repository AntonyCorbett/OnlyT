using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlyT.Utils
{
    public static class DateUtils
    {
        public static DateTime GetMondayOfThisWeek()
        {
            return GetMondayOfWeek(DateTime.Today);
        }

        public static DateTime GetMondayOfWeek(DateTime theDate)
        {
            int daysToDeduct = (7 + (theDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            return theDate.AddDays(-daysToDeduct).Date;
        }
    }
}
