namespace OnlyT.Services.TalkSchedule;

using System;
using System.Collections.Generic;
using Models;
using Options;
    
internal static class TalkScheduleManual
{
    public static List<TalkScheduleItem> Read(IOptionsService optionsService)
    {
        return
        [
            new TalkScheduleItem(10000, "Manual", string.Empty, string.Empty)
            {
                OriginalDuration = TimeSpan.FromMinutes(30),
                Editable = true,
                BellApplicable = optionsService.Options.IsBellEnabled,
                AutoBell = optionsService.Options.AutoBell,
            }
        ];
    }
}