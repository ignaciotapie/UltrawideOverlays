using Avalonia.Data.Converters;
using System;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Converters
{
    public class PathToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string path)
            {
                return ImageHelper.LoadFromResource(ImageHelper.GetUriFromPath(path));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
