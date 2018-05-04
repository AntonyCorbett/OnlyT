namespace OnlyT.WebServer.Throttling
{
    internal enum ApiRequestType
    {
        /// <summary>
        /// Unknown request.
        /// </summary>
        Unknown,

        /// <summary>
        /// The clock webpage.
        /// </summary>
        ClockPage,

        /// <summary>
        /// The clock webpage data.
        /// </summary>
        ClockData,

        /// <summary>
        /// Timer status information.
        /// </summary>
        Timer,

        /// <summary>
        /// Timer start request.
        /// </summary>
        TimerControlStart,

        /// <summary>
        /// Timer stop request.
        /// </summary>
        TimerControlStop,

        /// <summary>
        /// Request for version.
        /// </summary>
        Version,

        /// <summary>
        /// Play bell request.
        /// </summary>
        Bell,

        /// <summary>
        /// Datetime request.
        /// </summary>
        DateTime,

        /// <summary>
        /// System information request.
        /// </summary>
        System
    }
}
