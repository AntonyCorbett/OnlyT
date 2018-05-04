namespace OnlyT.ValueConverters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class BoolToCursorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if ((bool)value)
                {
                    return "Hand";
                }
            }

            return "Default";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
