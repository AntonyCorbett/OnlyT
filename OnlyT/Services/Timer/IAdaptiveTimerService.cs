﻿namespace OnlyT.Services.Timer
{
    using System;

    public interface IAdaptiveTimerService
    {
        TimeSpan? CalculateAdaptedDuration(int itemId);

        TimeSpan? CalculateMeetingOverrun(int talkId);
    }
}
