namespace OnlyT.Services.TalkSchedule
{
    using System;
    using System.Collections.Generic;
    using Models;
    using Options;

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
