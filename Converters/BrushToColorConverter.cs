using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Globalization;

namespace UltrawideOverlays.Converters
{
    public class BrushToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return Convert(value, targetType);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return ConvertBack(value, targetType);
        }

        public static object? Convert(object? value, Type? targetType)
        {
            if (targetType == typeof(Color) && value is ISolidColorBrush brush)
            {
                return brush.Color;
            }

            return value;
        }

        public static object? ConvertBack(object? value, Type? targetType)
        {
            if (targetType == typeof(IBrush) && value is Color c)
            {
                return new ImmutableSolidColorBrush(c);
            }

            return value;
        }
    }
}
