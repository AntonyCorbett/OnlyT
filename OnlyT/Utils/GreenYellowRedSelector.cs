namespace OnlyT.Utils
{
    using System.Windows.Media;

    /// <summary>
    /// Returns an appropriate brush based on time remaining:
    /// Red if overtime; Yellow if 30 secs or less to go; otherwise Green
    /// </summary>
    internal static class GreenYellowRedSelector
    {
        private static readonly Brush GreenBrush = new SolidColorBrush(Colors.Chartreuse);
        private static readonly Brush YellowBrush = new SolidColorBrush(Colors.Yellow);
        private static readonly Brush RedBrush = new SolidColorBrush(Colors.Red);

        /// <summary>
        /// Gets a brush (red, yellow or green)
        /// </summary>
        /// <param name="secsRemaining">Seconds remaining in the talk (can be negative)</param>
        /// <returns>A brush to use when drawing time values etc.</returns>
        public static Brush GetBrushForTimeRemaining(int secsRemaining)
        {
            if (secsRemaining <= 0)
            {
                return RedBrush;
            }

            if (secsRemaining <= 60)
            {
                return YellowBrush;
            }

            return GreenBrush;
        }

        public static Brush GetGreenBrush()
        {
            return GreenBrush;
        }
    }
}
