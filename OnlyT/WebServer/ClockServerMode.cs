namespace OnlyT.WebServer
{
    public enum ClockServerMode
    {
        /// <summary>
        /// The nameplate.
        /// </summary>
        Nameplate,

        /// <summary>
        /// The time of day.
        /// </summary>
        TimeOfDay,

        /// <summary>
        /// The timer.
        /// </summary>
        Timer,

        /// <summary>
        /// Persisting the final student time after the timer has stopped.
        /// </summary>
        Persist
    }
}
