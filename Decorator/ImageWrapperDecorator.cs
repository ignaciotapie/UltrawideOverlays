using System;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
using UltrawideOverlays.Wrappers;

namespace UltrawideOverlays.Decorator
{
    public class ImageWrapperDecorator
    {
        private readonly ImageCacheService _imageService;
        public ImageWrapperDecorator(ImageCacheService imageService)
        {
            _imageService = imageService;
        }

        public ImageWrapper CreateImageWrapper(ImageModel? model, string imagePath)
        {
            var handle = _imageService.AddImage(imagePath);
            return new ImageWrapper(model, handle);
        }

        public OverlayWrapper CreateOverlayWrapper(OverlayDataModel? model, string imagePath)
        {
            var handle = _imageService.AddImage(imagePath);
            if (handle == null)
            {
                throw new ArgumentException("Overlay not found in cache", nameof(imagePath));
            }
            return new OverlayWrapper(model, handle);
        }
    }
}
