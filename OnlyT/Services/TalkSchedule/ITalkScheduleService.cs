using System.Collections.Generic;
using OnlyT.Models;

namespace OnlyT.Services.TalkSchedule
{
    public interface ITalkScheduleService
    {
        IEnumerable<TalkScheduleItem> GetTalkScheduleItems();
        TalkScheduleItem GetTalkScheduleItem(int id);
        int GetNext(int currentTalkId);
        void Reset();
    }
}
