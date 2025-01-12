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

            cfg.DisplayOptions.Width = 300;

            cfg.Dispatcher = Application.Current.Dispatcher;
        });
            
        _optionsService = optionsService;
    }

    public void NotifyOfBadTiming(TimeSpan overrun)
    {
        if (_optionsService.Options.OverrunNotifications)
        {
            var absMins = Math.Abs(Math.Round(overrun.TotalMinutes));

            var msg = overrun < TimeSpan.Zero
                ? string.Format(Properties.Resources.OVERRUN_MSG, absMins)
                : string.Format(Properties.Resources.UNDERRUN_MSG, absMins);

            SendBadTimingNotification(msg);
        }
    }

    private void SendBadTimingNotification(string msg)
    {
        _notifier.ShowWarning(msg, new MessageOptions
        {
            ShowCloseButton = true,
            FreezeOnMouseEnter = false,
            FontSize = 14,
        });
    }

    public void Shutdown()
    {
        _notifier.Dispose();
    }
}