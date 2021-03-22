namespace OnlyT.Utils
{
    using System.Windows.Media;
    using Models;
    using Services.Options;

    internal static class DurationStringGeneration
    {
        private static readonly Brush DurationBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f3dcbc"));
        private static readonly Brush DurationDimBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#bba991"));

        public static DurationStringProperties Get(AdaptiveMode adaptiveMode, TalkScheduleItem? talk)
        {
            var result = new DurationStringProperties();

            if (talk != null)
            {
                result.Duration1String = TimeFormatter.FormatTimerDisplayString((int)talk.OriginalDuration.TotalSeconds);
                result.Duration1Tooltip = Properties.Resources.DURATION_ORIGINAL;

                if (talk.ModifiedDuration != null)
                {
                    result.Duration2String = TimeFormatter.FormatTimerDisplayString((int)talk.ModifiedDuration.Value.TotalSeconds);
                    result.Duration2Tooltip = Properties.Resources.DURATION_MODIFIED;
                    
                    var showAdaptedDuration = talk.AdaptedDuration != null &&
                                               (adaptiveMode == AdaptiveMode.TwoWay ||
                                                talk.AdaptedDuration.Value < talk.ModifiedDuration.Value);

                    if (showAdaptedDuration)
                    {
                        result.Duration3String = TimeFormatter.FormatTimerDisplayString((int)talk.AdaptedDuration!.Value.TotalSeconds);
                        result.Duration3Tooltip = Properties.Resources.DURATION_ADAPTED;
                    }
                    else
                    {
                        result.Duration3String = string.Empty;
                    }
                }
                else if (talk.AdaptedDuration != null)
                {
                    result.Duration2String = TimeFormatter.FormatTimerDisplayString((int)talk.AdaptedDuration.Value.TotalSeconds);
                    result.Duration2Tooltip = Properties.Resources.DURATION_ADAPTED;
                    result.Duration3String = string.Empty;
                }
                else
                {
                    result.Duration2String = string.Empty;
                    result.Duration3String = string.Empty;
                }
            }
            else
            {
                result.Duration1String = string.Empty;
                result.Duration2String = string.Empty;
                result.Duration3String = string.Empty;
            }

            result.Duration1Colour = DurationDimBrush;
            result.Duration2Colour = DurationDimBrush;
            result.Duration3Colour = DurationDimBrush;

            if (!string.IsNullOrEmpty(result.Duration3String))
            {
                result.Duration3Colour = DurationBrush;
            }
            else if (!string.IsNullOrEmpty(result.Duration2String))
            {
                result.Duration2Colour = DurationBrush;
            }
            else
            {
                result.Duration1Colour = DurationBrush;
            }

            return result;
        }
    }
}
