namespace OnlyT.Services.TalkSchedule
{
    /// <summary>
    /// Talk types for automatic mode
    /// </summary>
    public enum TalkTypesAutoMode
    {
        /// <summary>
        /// Unknown item.
        /// </summary>
        Unknown,

        /// <summary>
        /// Opening comments.
        /// </summary>
        OpeningComments = 1000,

        /// <summary>
        /// Treasures item.
        /// </summary>
        TreasuresTalk,

        /// <summary>
        /// Digging item.
        /// </summary>
        DiggingTalk,

        /// <summary>
        /// Bible reading.
        /// </summary>
        Reading,

        /// <summary>
        /// Ministry item 1.
        /// </summary>
        MinistryItem1 = 2000,

        /// <summary>
        /// Ministry item 2.
        /// </summary>
        MinistryItem2,

        /// <summary>
        /// Ministry item 3.
        /// </summary>
        MinistryItem3,

        /// <summary>
        /// Ministry item 4.
        /// </summary>
        MinistryItem4,

        /// <summary>
        /// Living item 1.
        /// </summary>
        LivingPart1 = 3000,

        /// <summary>
        /// Living item 2.
        /// </summary>
        LivingPart2,

        /// <summary>
        /// Congregation Bible study.
        /// </summary>
        CongBibleStudy = 4000,

        /// <summary>
        /// CO Service talk.
        /// </summary>
        CircuitServiceTalk = 5000,
        
        /// <summary>
        /// Concluding comments.
        /// </summary>
        ConcludingComments = 6000,

        /// <summary>
        /// Public talk.
        /// </summary>
        PublicTalk = 7000,

        /// <summary>
        /// WT study.
        /// </summary>
        Watchtower = 8000
    }
}
