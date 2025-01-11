namespace OnlyT.Services.Reminders;

public interface IReminderService
{
    void Shutdown();

    void SendBadTimingNotification(string msg);
}