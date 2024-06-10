namespace OnlyT.Services.Reminders;

public interface IReminderService
{
    void Send(string msg);

    void Shutdown();
}