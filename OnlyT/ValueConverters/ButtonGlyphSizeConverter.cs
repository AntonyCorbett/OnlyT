﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace OnlyT.ValueConverters;

public class ButtonGlyphSizeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((double) value * 0.8);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}