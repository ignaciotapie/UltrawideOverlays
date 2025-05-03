using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace UltrawideOverlays.Converters
{
    public class ScaleToTransformConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double scale)
            {
                return new ScaleTransform(scale, scale);
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is ScaleTransform scaleTransform)
            {
                return scaleTransform.ScaleX;
            }
            return 1.0;
        }
    }
}
