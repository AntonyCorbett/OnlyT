using System;
using System.Windows;
using OnlyT.Services.Options;
using ToastNotifications.Core;
using ToastNotifications;
using ToastNotifications.Messages;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using CommunityToolkit.Mvvm.Messaging;
using OnlyT.ViewModel.Messages;

namespace OnlyT.Services.OverrunNotificationService;

public class OverrunService : IOverrunService
{
    private readonly Notifier _notifier;
    private readonly IOptionsService _optionsService;

    public OverrunService(IOptionsService optionsService)
    {
        // https://github.com/rafallopatka/ToastNotifications/blob/master-v2/Docs/Configuration.md

        _notifier = new Notifier(cfg =>
        {
            cfg.PositionProvider = new PrimaryScreenPositionProvider(Corner.TopRight, 20, 20);

            cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                notificationLifetime: TimeSpan.FromSeconds(10),
                maximumNotificationCount: MaximumNotificationCount.FromCount(2));

            cfg.DisplayOptions.Width = 200;
            
            cfg.Dispatcher = Application.Current.Dispatcher;
        });
            
        _optionsService = optionsService;
    }

    public void NotifyOfBadTiming(TimeSpan overrun)
    {
        if (_optionsService.Options.OverrunNotifications)
        {
            var absMins = Math.Abs(Math.Round(overrun.TotalMinutes));

            var isOverrun = overrun < TimeSpan.Zero;

            var msg = isOverrun
                ? string.Format(Properties.Resources.OVERRUN_MSG, (int)absMins)
                : string.Format(Properties.Resources.UNDERRUN_MSG, (int)absMins);

            SendBadTimingNotification(msg, isOverrun, (int)absMins);
        }
    }

    private void SendBadTimingNotification(string msg, bool isOverrun, int mins)
    {
        _notifier.ShowCustomOverrunMessage(msg, isOverrun, mins, new MessageOptions
        {
            FreezeOnMouseEnter = true,
            UnfreezeOnMouseLeave = true
        });
    }

    public void Shutdown()
    {
        _notifier.Dispose();
    }
}