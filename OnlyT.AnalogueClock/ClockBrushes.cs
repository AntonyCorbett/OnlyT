namespace OnlyT.AnalogueClock
{
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Effects;

    internal static class ClockBrushes
    {
        public static LinearGradientBrush GetOuterDialBrush()
        {
            var result = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString("#FFB1B2AA"), 0),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#FFACACA2"), 1),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#FFD1D1D1"), 0.673),
                },
                new Point(0.5, 0.0),
                new Point(0.0, 0.5))
            {
                MappingMode = BrushMappingMode.RelativeToBoundingBox
            };

            return result;
        }

        public static SolidColorBrush GetOuterDialBrushFlat()
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFACACA2"));
        }

        public static LinearGradientBrush GetMiddleDialBrush()
        {
            var result = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString("#FF151515"), 0),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#FF130101"), 1),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#FFD1D1D1"), 0.673),
                },
                new Point(0.5, 0.0),
                new Point(0.0, 0.5))
            {
                MappingMode = BrushMappingMode.RelativeToBoundingBox
            };

            return result;
        }

        public static SolidColorBrush GetMiddleDialBrushFlat()
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF130101"));
        }

        public static DropShadowEffect GetCentrePointDropShadow()
        {
            return new DropShadowEffect { ShadowDepth = -15 };
        }
    }
}
