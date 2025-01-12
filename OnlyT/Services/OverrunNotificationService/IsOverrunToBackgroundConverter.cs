using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace OnlyT.Services.OverrunNotificationService;

public class IsOverrunToBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null && (bool)value)
        {
            return Brushes.DarkRed;
        }

        return Brushes.DarkGreen;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
}