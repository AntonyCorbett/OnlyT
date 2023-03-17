using System;

namespace OnlyT.Models;

internal sealed class DayOfWeekAndUserSuppliedDayName
{
    public DayOfWeekAndUserSuppliedDayName(DayOfWeek dayOfWeek, string userSuppliedDayName)
    {
        DayOfWeek = dayOfWeek;
        UserSuppliedDayName = userSuppliedDayName;
    }

    public DayOfWeek? DayOfWeek { get; }

    public string UserSuppliedDayName { get; }
}