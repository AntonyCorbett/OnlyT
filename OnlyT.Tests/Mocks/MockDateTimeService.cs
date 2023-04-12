namespace OnlyT.Tests.Mocks;

using System;
using OnlyT.Common.Services.DateTime;
    
internal sealed class MockDateTimeService : IDateTimeService
{
    private DateTime _value;

    public void Set(DateTime dt)
    {
        _value = dt;
    }

    public void Add(TimeSpan timeSpan)
    {
        _value += timeSpan;
    }

    public DateTime Now()
    {
        if (_value == default)
        {
            throw new NotSupportedException("date is not set");
        }

        return _value;
    }

    public DateTime UtcNow()
    {
        return Now();
    }

    public DateTime Today()
    {
        return Now().Date;
    }
}