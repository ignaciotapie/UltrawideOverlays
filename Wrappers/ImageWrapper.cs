using UltrawideOverlays.Models;
using static UltrawideOverlays.Services.ImageCacheService;

namespace UltrawideOverlays.Wrappers
{
    public class ImageWrapper : BaseCachedBitmapWrapper
    {
        public ImageModel? Model { get; }

        public string? Name => Model?.ImageName;

        public ImageWrapper(ImageModel? model, CacheBitmapHandle handle)
        {
            Model = model;
            BitmapHandle = handle;
        }

        public override string ToString()
        {
            return Model?.ToString() ?? "ImageWrapper";
        }
    }
}
