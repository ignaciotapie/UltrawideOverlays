using Avalonia;
using Avalonia.Data.Converters;
using System;

namespace UltrawideOverlays.Converters
{
    public class NullToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            // If the value is null, return a default image or a placeholder
            if (value == null)
            {
                if (Application.Current?.TryGetResource("ClippingMask", null, out var resourceValue) == true)
                {
                    return resourceValue;
                }
                return null;
            }
            return value; // Otherwise, return the original value
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not implemented.");
        }
    }
}
