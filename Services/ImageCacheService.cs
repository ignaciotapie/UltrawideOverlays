using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Services
{
    public class ImageCacheService
    {
        public class CacheBitmapHandle : IDisposable
        {
            public Bitmap Bitmap { get; internal set; }
            public DateTime LastAccessTime { get; set; }
            public long SizeBytes { get; }
            public int RefCount { get; private set; }
            public void IncrementRef() => RefCount++;
            public void DecrementRef() => RefCount = Math.Max(0, RefCount - 1);
            public bool IsInUse => RefCount > 0;
            public void UpdateLastAccessTime() => LastAccessTime = DateTime.UtcNow;

            public bool IsInvalidated { get; internal set; } = false;

            public bool IsDisposed = false;
            public void Invalidate()
            {
                IsInvalidated = true;
                if (!IsInUse)
                {
                    Dispose();
                }
            }

            public CacheBitmapHandle(Bitmap bitmap)
            {
                Bitmap = bitmap;
                LastAccessTime = DateTime.UtcNow;
                SizeBytes = EstimateBitmapSize(bitmap);
                RefCount = 1;
            }
            ~CacheBitmapHandle()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (IsDisposed) { return; }

                IsDisposed = true;
                Bitmap.Dispose();
                Bitmap = null;
                GC.SuppressFinalize(this);
            }
        }

        private readonly Dictionary<string, CacheBitmapHandle> _cache;
        private readonly object _lock;
        private readonly Timer _cleanupTimer;
        private long _currentMemoryUsage = 0;

        //Configuration
        private const int MaxCacheSize = 10; //Maximum number of items
        private const long MaxMemoryBytes = 100 /*MB*/ * 1024 * 1024;
        private readonly TimeSpan _inactivityTimeout = TimeSpan.FromSeconds(30); //Flag bitmap as disposable after X time unused
        private readonly TimeSpan _cleanupTimePeriod = TimeSpan.FromSeconds(10); //Cleanup interval

        public ImageCacheService()
        {
            // Set up periodic cleanup
            _cleanupTimer = new Timer(CleanupCallback, null, _cleanupTimePeriod, _cleanupTimePeriod);
            _cache = [];
            _lock = new object();
        }

        public CacheBitmapHandle? AddOrGetHandle(string path)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(path, out var cacheEntry) && !cacheEntry.IsInvalidated)
                {
                    cacheEntry.IncrementRef();
                    cacheEntry.UpdateLastAccessTime();
                    return cacheEntry;
                }
                else
                {
                    return AddImage(path);
                }
            }
        }

        public void ReleaseImage(string path)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(path, out var cacheEntry))
                {
                    cacheEntry.DecrementRef();
                    cacheEntry.UpdateLastAccessTime();
                }
            }
        }

        public CacheBitmapHandle? AddImage(string path)
        {
            lock (_lock)
            {
                var isCached = _cache.TryGetValue(path, out var cacheHandle);
                if (FileHandlerUtil.IsValidImagePath(path) && (!isCached || cacheHandle.IsInvalidated))
                {
                    try
                    {
                        using var stream = File.OpenRead(path);
                        var bitmap = new Bitmap(stream);
                        return AddToCache(path, bitmap);
                    }
                    catch (Exception ex)
                    {
                        // Log the exception or handle it as needed
                        Console.WriteLine($"Error loading image from path '{path}': {ex.Message}");
                        return null;
                    }
                }
                else if (isCached)
                {
                    return cacheHandle;
                }
                else
                {
                    // Fallback
                    return null;
                }
            }
        }
        private CacheBitmapHandle AddToCache(string path, Bitmap bitmap)
        {
            lock (_lock)
            {
                var newEntry = new CacheBitmapHandle(bitmap);

                //Try to free space inmediately if needed
                if ((_currentMemoryUsage + newEntry.SizeBytes > MaxMemoryBytes || _cache.Count >= MaxCacheSize) && _cache.Count > 0)
                {
                    TryCleanupUnusedItems();
                }

                _cache[path] = newEntry;
                _currentMemoryUsage += newEntry.SizeBytes;
                return newEntry;
            }
        }

        public void TryCleanupUnusedItems()
        {
            Debug.WriteLine("Removing items not in use from cache to free space...");

            lock (_lock)
            {
                var keysToRemove = new List<string>();
                var removalAmt = 0;
                long removalMemoryUsage = 0;

                foreach (var item in _cache)
                {
                    if (!item.Value.IsInUse)
                    {
                        keysToRemove.Add(item.Key);
                        removalAmt++;
                        removalMemoryUsage += item.Value.SizeBytes;
                    }
                    if (_currentMemoryUsage - removalMemoryUsage < MaxMemoryBytes && _cache.Count - removalAmt < MaxCacheSize) break;
                }

                if (keysToRemove.Count > 0)
                {
                    foreach (var key in keysToRemove)
                    {
                        RemoveEntry(key);
                    }
                }
                else
                {
                    Debug.WriteLine("No unused items found to remove from cache.");
                }
            }
        }

        private void RemoveEntry(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                Debug.WriteLine($"Removing image from cache: {key}");
                _currentMemoryUsage -= entry.SizeBytes;
                entry.Dispose();
                _cache.Remove(key);
            }
        }

        private void CleanupCallback(object? state)
        {
            lock (_lock)
            {
                var cutoffTime = DateTime.UtcNow - _inactivityTimeout;
                var keysToRemove = new List<string>();

                // Identify items to remove
                foreach (var kvp in _cache)
                {
                    if (!kvp.Value.IsInUse && kvp.Value.LastAccessTime < cutoffTime)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                // Remove the inactive items
                foreach (var key in keysToRemove)
                {
                    RemoveEntry(key);
                }
            }
        }
        private static long EstimateBitmapSize(Bitmap bitmap)
        {
            return bitmap.PixelSize.Width * bitmap.PixelSize.Height * 4L;
        }

        public void InvalidateCache(string path)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(path, out var cacheEntry))
                {
                    cacheEntry.Invalidate();
                    _currentMemoryUsage -= cacheEntry.SizeBytes;
                    Debug.WriteLine($"Invalidated and removed image from cache: {path}");
                }
            }
        }
    }
}
