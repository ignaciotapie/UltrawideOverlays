using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using static UltrawideOverlays.Services.ImageCacheService;

namespace UltrawideOverlays.Wrappers
{
    public abstract class BaseCachedBitmapWrapper : ObservableObject, IDisposable
    {
        public CacheBitmapHandle? BitmapHandle { get; protected set; }
        public Bitmap? ImageSource => BitmapHandle?.Bitmap;
        public bool IsDisposed { get; private set; }

        ~BaseCachedBitmapWrapper()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            BitmapHandle?.DecrementRef();
            BitmapHandle = null;
            GC.SuppressFinalize(this);
        }
    }
}
