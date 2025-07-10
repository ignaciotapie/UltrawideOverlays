using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using System;
using System.Globalization;

namespace UltrawideOverlays.Converters
{
    public class StringToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string colorString)
            {
                try
                {
                    // Attempt to parse the color from the string
                    var color = Color.Parse(colorString);
                    return new ImmutableSolidColorBrush(color);
                }
                catch (Exception)
                {
                    // If parsing fails, return a default color (e.g., Transparent)
                    return Brushes.AliceBlue;
                }
            }

            // If value is not a string, return a default color
            return Brushes.AliceBlue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IBrush brush)
            {
                // Convert the brush back to a string representation of the color
                return brush.ToString();
            }

            // If value is not a brush, return null or a default string
            return null;
        }
    }
}
