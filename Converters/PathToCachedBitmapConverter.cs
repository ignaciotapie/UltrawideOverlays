using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;

namespace UltrawideOverlays.Converters
{
    public class PathToCachedBitmapConverter : IValueConverter, IDisposable
    {
        public static readonly PathToCachedBitmapConverter Instance = new();

        private readonly object _cacheLock = new();
        private readonly Dictionary<string, CacheEntry> _bitmapCache = new();
        private readonly Timer _cleanupTimer;

        private long _currentMemoryUsage = 0;

        //Configuration
        private const int MaxCacheSize = 15; //Maximum number of items
        private const long MaxMemoryBytes = 100 * 1024 * 1024;
        private readonly TimeSpan _inactivityTimeout = TimeSpan.FromSeconds(30); //Flag bitmap as disposable after X mins unused
        private readonly TimeSpan _cleanupTimePeriod = TimeSpan.FromSeconds(10); //Cleanup interval

        private class CacheEntry
        {
            public Bitmap Bitmap { get; }
            public DateTime LastAccessTime { get; set; }
            public long SizeBytes { get; }

            public CacheEntry(Bitmap bitmap)
            {
                Bitmap = bitmap;
                LastAccessTime = DateTime.UtcNow;
                SizeBytes = EstimateBitmapSize(bitmap);
            }

            private static long EstimateBitmapSize(Bitmap bitmap)
            {
                return bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4L; // 4 bytes per pixel (RGBA)
            }
        }

        public PathToCachedBitmapConverter()
        {
            // Set up periodic cleanup (runs every minute)
            _cleanupTimer = new Timer(CleanupCallback, null, _cleanupTimePeriod, _cleanupTimePeriod);
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
            {
                if (Application.Current?.TryFindResource("ClippingMaskBitmap", out object? resource) == true)
                {
                    return resource as Bitmap;
                }
                return AvaloniaProperty.UnsetValue;
            }

            try
            {
                var fullPath = Path.GetFullPath(path);
                bool shouldCache = parameter == null || (parameter is bool b && b);

                lock (_cacheLock)
                {
                    // Check cache first
                    if (_bitmapCache.TryGetValue(fullPath, out var entry))
                    {
                        entry.LastAccessTime = DateTime.UtcNow;
                        return entry.Bitmap;
                    }

                    if (File.Exists(fullPath))
                    {
                        using var stream = File.OpenRead(fullPath);
                        var bitmap = new Bitmap(stream);

                        if (shouldCache)
                        {
                            AddToCache(fullPath, bitmap);
                        }

                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading bitmap: {ex.Message}");
            }

            return AvaloniaProperty.UnsetValue;
        }

        private void AddToCache(string path, Bitmap bitmap)
        {
            lock (_cacheLock)
            {
                var newEntry = new CacheEntry(bitmap);

                //Ensure we have space
                while ((_currentMemoryUsage + newEntry.SizeBytes > MaxMemoryBytes || _bitmapCache.Count >= MaxCacheSize) && _bitmapCache.Count > 0)
                {
                    RemoveOldestItem();
                }

                //Don't cache if still too big
                if (_currentMemoryUsage + newEntry.SizeBytes > MaxMemoryBytes)
                    return;

                _bitmapCache[path] = newEntry;
                _currentMemoryUsage += newEntry.SizeBytes;
            }
        }

        private void RemoveOldestItem()
        {
            string? oldestKey = null;
            DateTime oldestTime = DateTime.UtcNow;

            foreach (var kvp in _bitmapCache)
            {
                if (kvp.Value.LastAccessTime < oldestTime)
                {
                    oldestTime = kvp.Value.LastAccessTime;
                    oldestKey = kvp.Key;
                }
            }

            if (oldestKey != null && _bitmapCache.TryGetValue(oldestKey, out var entry))
            {
                _currentMemoryUsage -= entry.SizeBytes;
                entry.Bitmap.Dispose();
                _bitmapCache.Remove(oldestKey);
            }
        }

        private void CleanupCallback(object? state)
        {
            lock (_cacheLock)
            {
                var cutoffTime = DateTime.UtcNow - _inactivityTimeout;
                var keysToRemove = new List<string>();

                // Identify items to remove
                foreach (var kvp in _bitmapCache)
                {
                    if (kvp.Value.LastAccessTime < cutoffTime)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                // Remove the inactive items
                foreach (var key in keysToRemove)
                {
                    if (_bitmapCache.TryGetValue(key, out var entry))
                    {
                        _currentMemoryUsage -= entry.SizeBytes;
                        entry.Bitmap.Dispose();
                        _bitmapCache.Remove(key);
                    }
                }
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public void ClearCache()
        {
            lock (_cacheLock)
            {
                foreach (var entry in _bitmapCache.Values)
                {
                    entry.Bitmap.Dispose();
                }
                _bitmapCache.Clear();

                _currentMemoryUsage = 0;
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            ClearCache();
            GC.SuppressFinalize(this);
        }

        ~PathToCachedBitmapConverter()
        {
            Dispose();
        }
    }
}