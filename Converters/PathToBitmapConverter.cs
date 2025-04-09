using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;

namespace UltrawideOverlays.Converters
{
    public class PathToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string path)
            {
                // Check if the path is a valid file path
                if (System.IO.File.Exists(path))
                {
                    // Use the BitmapLoader to load the image
                    return new Bitmap(path);
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
