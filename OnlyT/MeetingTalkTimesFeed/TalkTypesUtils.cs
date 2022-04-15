using OnlyT.Services.TalkSchedule;

namespace OnlyT.MeetingTalkTimesFeed
{
    internal static class TalkTypesUtils
    {
        public static TalkTypes GetMinistryTalkType(int index)
        {
            return index switch
            {
                0 => TalkTypes.Ministry1,
                1 => TalkTypes.Ministry2,
                2 => TalkTypes.Ministry3,
                3 => TalkTypes.Ministry4,
                _ => TalkTypes.Unknown
            };
        }

        public static TalkTypesAutoMode GetAutoModeMinistryTalkType(int index)
        {
            return index switch
            {
                0 => TalkTypesAutoMode.MinistryItem1,
                1 => TalkTypesAutoMode.MinistryItem2,
                2 => TalkTypesAutoMode.MinistryItem3,
                3 => TalkTypesAutoMode.MinistryItem4,
                _ => TalkTypesAutoMode.Unknown
            };
        }
    }
}
