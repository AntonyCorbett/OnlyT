using System;
using System.Collections.Generic;
using OnlyT.Models;
using OnlyT.Services.Options;

namespace OnlyT.Services.TalkSchedule
{
    internal class TalkScheduleManual
    {
        public static IEnumerable<TalkScheduleItem> Read(IOptionsService optionsService)
        {
            return new List<TalkScheduleItem>
            {
                new TalkScheduleItem
                {
                    Id = 10000,
                    OriginalDuration = TimeSpan.FromMinutes(30),
                    Editable = true,
                    Bell = optionsService.Options.IsBellEnabled,
                    Name = "Manual"
                }
            };
        }
    }
}
