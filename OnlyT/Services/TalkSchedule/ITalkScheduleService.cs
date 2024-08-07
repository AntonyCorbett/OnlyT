﻿namespace OnlyT.Services.TalkSchedule
{
    using System;
    using System.Collections.Generic;
    using Models;

    public interface ITalkScheduleService
    {
        IEnumerable<TalkScheduleItem> GetTalkScheduleItems();

        TalkScheduleItem? GetTalkScheduleItem(int id);

        int GetNext(int currentTalkId);

        void Reset();

        bool SuccessGettingAutoFeedForMidWeekMtg();

        void SetModifiedDuration(int talkId, TimeSpan? modifiedDuration);
    }
}
