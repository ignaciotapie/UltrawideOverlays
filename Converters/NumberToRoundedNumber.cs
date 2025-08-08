using Avalonia.Data.Converters;
using System;

namespace UltrawideOverlays.Converters
{
    public class NumberToRoundedNumber : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            var digits = 0;
            if (parameter is string digitsAmt)
            {
                digits = Int32.Parse(digitsAmt);
            }
            if (value is double d)
            {
                return Math.Round(d, digits);
            }
            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            //No need to do anything
            return value;
        }
    }
}
