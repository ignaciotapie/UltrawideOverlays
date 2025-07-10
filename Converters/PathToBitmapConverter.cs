using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace UltrawideOverlays.Converters
{
    public class PathToBitmapConverter : IValueConverter
    {
        public static readonly PathToBitmapConverter Converter = new();

        // Cache loaded bitmaps with full path key
        private readonly ConcurrentDictionary<string, Bitmap> _bitmapCache = new();

        public object Convert(object? value, Type targetType, object? saveInCache, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
                return AvaloniaProperty.UnsetValue;

            try
            {
                var fullPath = Path.GetFullPath(path);

                // Return cached if already loaded
                if (_bitmapCache.TryGetValue(fullPath, out var cached))
                    return cached;

                if (File.Exists(fullPath))
                {
                    var bitmap = new Bitmap(fullPath);
                    if ((saveInCache == null) || (saveInCache is bool save && save))
                    {
                        _bitmapCache[fullPath] = bitmap;
                    }
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not convert to Bitmap, Path: {path}, Msg: {ex.Message}");
            }

            return AvaloniaProperty.UnsetValue;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public void DisposeImage()
        {
            foreach (var bmp in _bitmapCache.Values)
            {
                bmp.Dispose();
            }

            _bitmapCache.Clear();
        }

        public static void CleanCache()
        {
            Converter.DisposeImage();
        }
    }
}
