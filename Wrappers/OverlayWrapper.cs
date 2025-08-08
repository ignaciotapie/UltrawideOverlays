using UltrawideOverlays.Models;
using static UltrawideOverlays.Services.ImageCacheService;

namespace UltrawideOverlays.Wrappers
{
    public class OverlayWrapper : BaseCachedBitmapWrapper
    {
        public OverlayDataModel? Model { get; }

        public OverlayWrapper(OverlayDataModel? model, CacheBitmapHandle handle)
        {
            Model = model;
            BitmapHandle = handle;
        }
    }
}
