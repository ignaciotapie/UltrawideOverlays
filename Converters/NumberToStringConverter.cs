using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace UltrawideOverlays.Converters
{
    public class NumberToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue.ToString(culture);
            }
            if (value is int intValue)
            {
                return intValue.ToString(culture);
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (int.TryParse(strValue, out int result))
                {
                    return result;
                }
                if (double.TryParse(strValue, out double doubleResult))
                {
                    return doubleResult;
                }
            }
            return null;
        }
    }

    static public class RoundedNumberToStringConverter
    {
        /// <summary>
        /// Gets a Converter that takes a number as input and converts it into a text representation
        /// </summary>
        public static FuncValueConverter<double?, string> Converter { get; } =
            new FuncValueConverter<double?, string>(number =>
            {
                if (number == null)
                    return string.Empty;

                // Check if the number is a valid double
                if (number is double doubleValue)
                {
                    // Round the number to 2 decimal places and convert it to string
                    return Math.Round(doubleValue, 2).ToString("F2", CultureInfo.InvariantCulture);
                }
                return string.Empty;
            });
    }
}
