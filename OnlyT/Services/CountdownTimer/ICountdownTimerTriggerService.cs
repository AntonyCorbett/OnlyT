namespace OnlyT.Services.CountdownTimer;

public interface ICountdownTimerTriggerService
{
    bool IsInCountdownPeriod(out int secondsOffset);

    void UpdateTriggerPeriods();
}