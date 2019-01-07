namespace OnlyT.MeetingTalkTimesFeed
{
    using OnlyT.Services.TalkSchedule;

    internal static class TalkTypesUtils
    {
        public static TalkTypes GetMinistryTalkTypePre2019(int index)
        {
            switch (index)
            {
                case 0:
                    return TalkTypes.Pre2019Ministry1;
                case 1:
                    return TalkTypes.Pre2019Ministry2;
                case 2:
                    return TalkTypes.Pre2019Ministry3;

                default:
                    return TalkTypes.Unknown;
            }
        }

        public static TalkTypes GetMinistryTalkType(int index)
        {
            switch (index)
            {
                case 0:
                    return TalkTypes.Ministry1;
                case 1:
                    return TalkTypes.Ministry2;
                case 2:
                    return TalkTypes.Ministry3;
                case 3:
                    return TalkTypes.Ministry4;

                default:
                    return TalkTypes.Unknown;
            }
        }

        public static TalkTypesAutoMode GetAutoModeMinistryTalkType(int index)
        {
            switch (index)
            {
                case 0:
                    return TalkTypesAutoMode.MinistryItem1;
                case 1:
                    return TalkTypesAutoMode.MinistryItem2;
                case 2:
                    return TalkTypesAutoMode.MinistryItem3;
                case 3:
                    return TalkTypesAutoMode.MinistryItem4;

                default:
                    return TalkTypesAutoMode.Unknown;
            }
        }
    }
}
