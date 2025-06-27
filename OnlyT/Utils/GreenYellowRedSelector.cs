namespace OnlyT.Utils
{
    using System.Windows.Media;

    /// <summary>
    /// Returns an appropriate brush based on time remaining:
    /// Red if overtime; Yellow if 30 secs or less to go; otherwise Green
    /// </summary>
    internal static class GreenYellowRedSelector
    {
        private static readonly SolidColorBrush GreenBrush = new(Colors.Chartreuse);
        private static readonly SolidColorBrush YellowBrush = new(Colors.Yellow);
        private static readonly SolidColorBrush RedBrush = new(Colors.Red);

        /// <summary>
        /// Gets a brush (red, yellow or green)
        /// </summary>
        /// <param name="secsRemaining">Seconds remaining in the talk (can be negative)</param>
        /// <param name="closingSecs">Closing secs duration (default 30)</param>
        /// <returns>A brush to use when drawing time values etc.</returns>
        public static SolidColorBrush GetBrushForTimeRemaining(int secsRemaining, int closingSecs)
        {
            if (secsRemaining <= 0)
            {
                return RedBrush;
            }

            if (secsRemaining <= closingSecs)
            {
                return YellowBrush;
            }

            return GreenBrush;
        }

        public static SolidColorBrush GetGreenBrush()
        {
            return GreenBrush;
        }
    }
}
