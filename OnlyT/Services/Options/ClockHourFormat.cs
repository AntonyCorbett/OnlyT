namespace OnlyT.Services.Options
{
    public enum ClockHourFormat
    {
        /// <summary>
        /// 12hr clock format, e.g. "3:00".
        /// </summary>
        Format12,

        /// <summary>
        /// 12hr clock format with leading hr zero, e.g. "03:00".
        /// </summary>
        Format12LeadingZero,

        /// <summary>
        /// 24hr clock format, e.g. "15:00".
        /// </summary>
        Format24,

        /// <summary>
        /// 24hr clock format with leading hr zero, e.g. "06:00".
        /// </summary>
        Format24LeadingZero,

        /// <summary>
        /// 12hr clock format, with am/pm e.g. "3:00pm".
        /// </summary>
        Format12AMPM,

        /// <summary>
        /// 12hr clock format, with leading zero and am/pm e.g. "03:00pm".
        /// </summary>
        Format12LeadingZeroAMPM
    }
}
