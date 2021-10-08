﻿namespace OnlyT.Utils
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
            var daysToDeduct = (7 + (theDate.DayOfWeek - DayOfWeek.Monday)) % 7;
            return theDate.AddDays(-daysToDeduct).Date;
        }

        public static DateTime GetNearestMinute(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
        }

        public static DateTime GetNearestQuarterOfAnHour(DateTime value)
        {
            var mins = value.Minute;
            int minsAdjust;

            var minsLong = mins % 15;
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
