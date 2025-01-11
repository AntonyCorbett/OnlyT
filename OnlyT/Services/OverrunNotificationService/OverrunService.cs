using System;
using OnlyT.Services.Reminders;

namespace OnlyT.Services.OverrunNotificationService;

public class OverrunService(IReminderService reminderService) : IOverrunService
{
    public void NotifyOfBadTiming(TimeSpan overrun)
    {
        var absMins = Math.Abs(Math.Round(overrun.TotalMinutes));

        var msg = overrun < TimeSpan.Zero
            ? string.Format(Properties.Resources.OVERRUN_MSG, absMins)
            : string.Format(Properties.Resources.UNDERRUN_MSG, absMins);

        reminderService.SendBadTimingNotification(msg);
    }
}