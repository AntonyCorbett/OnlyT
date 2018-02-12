using System;

namespace OnlyT.ValueConverters
{
    using System.Globalization;
    using System.Windows.Data;

    public class ColWidthPercentageToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                int percentage = (int)value;
                return $"{percentage}*";
            }

            return "0*";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
