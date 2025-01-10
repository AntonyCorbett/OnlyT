using System;

namespace OnlyT.Services.OverrunNotificationService;

public interface IOverrunService
{
    void NotifyOfBadTiming(TimeSpan overrun);
}