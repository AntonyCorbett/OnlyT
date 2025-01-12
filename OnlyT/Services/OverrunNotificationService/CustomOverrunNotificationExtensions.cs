using ToastNotifications;
using ToastNotifications.Core;

namespace OnlyT.Services.OverrunNotificationService;

public static class CustomOverrunNotificationExtensions
{
    public static void ShowCustomOverrunMessage(
        this Notifier notifier, string body, bool isOverrun, int mins, MessageOptions? messageOptions = null)
    {
        notifier.Notify(() => 
            new CustomOverrunNotification(body, isOverrun, mins, messageOptions));
    }
}