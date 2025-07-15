using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Color = Avalonia.Media.Color;

namespace UltrawideOverlays.Converters
{
    public class NumberToColorConverter : IValueConverter
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
            var outColor = default(Color);
            if (value is string str && uint.TryParse(str, out var number))
            {
                outColor = Color.FromUInt32(number);
            }
            else if (value is Color color)
            {
                outColor = color;
            }
            else if (targetType == typeof(Color) && value is uint colorNum)
            {
                outColor = Color.FromUInt32(colorNum);
            }
            return outColor;
        }

        public static object? ConvertBack(object? value, Type? targetType)
        {
            var outString = default(string);
            if (targetType == typeof(string) && value is Color color)
            {
                outString = color.ToUInt32().ToString();
            }

            return outString;
        }
    }
}
