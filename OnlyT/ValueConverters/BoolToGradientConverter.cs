namespace OnlyT.ValueConverters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;

    public class BoolToGradientConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value)
            {
                return CreateBrush(true);
            }

            return CreateBrush(false);
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        private static LinearGradientBrush CreateBrush(bool useGradient)
        {
            var result = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1),
                Opacity = 1
            };

            var color = useGradient
                ? (Color)ColorConverter.ConvertFromString("#FF5D4343")
                : Colors.Black;

            result.GradientStops.Add(new GradientStop(Colors.Black, 1));
            result.GradientStops.Add(new GradientStop(color, 0));

            return result;
        }
    }
}
