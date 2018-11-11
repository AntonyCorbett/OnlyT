namespace OnlyT.Utils
{
    using System;

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

        public static DateTime GetNearestQuarterOfAnHour(DateTime value)
        {
            int mins = value.Minute;
            int minsAdjust;

            int minsLong = mins % 15;
            if (minsLong <= 10)
            {
                minsAdjust = -minsLong;
            }
            else
            {
                minsAdjust = 15 - minsLong;
            }

            var newValue = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
            return newValue.AddMinutes(minsAdjust);
        }
    }
}
