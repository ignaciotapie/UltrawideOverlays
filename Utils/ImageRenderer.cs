using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ImageMagick;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Utils;

public static class ImageRenderer
{
    public static RenderTargetBitmap RenderImagesToBitmap(IEnumerable<ImageModel> imageModels, IEnumerable<ClippingMaskModel> clippingMaskModels, PixelSize outputSize)
    {
        var bitmap = new RenderTargetBitmap(outputSize);

        using (var ctx = bitmap.CreateDrawingContext())
        {
            ctx.PushGeometryClip(CreateGeometryFromClippingMasks(clippingMaskModels, outputSize));
            foreach (var im in imageModels)
            {
                var props = im.ImageProperties;

                if (!props.IsVisible || !FileHandlerUtil.IsValidImagePath(im.ImagePath))
                    continue;

                using (var image = GetPropertyReadyBitmap(im))
                {
                    ctx.DrawImage(image, new Rect(props.PositionX, props.PositionY, image.Size.Width, image.Size.Height));
                };
            }
        }
        return bitmap;
    }

    private static Geometry CreateGeometryFromClippingMasks(IEnumerable<ClippingMaskModel> masks, PixelSize outputSize)
    {
        Geometry geometry = new RectangleGeometry(new Rect(0, 0, outputSize.Width, outputSize.Height));

        foreach (var mask in masks)
        {
            var rect = new RectangleGeometry(
                new Rect(
                    mask.ImageProperties.PositionX,
                    mask.ImageProperties.PositionY,
                    mask.ImageProperties.Width,
                    mask.ImageProperties.Height
                )
            );
            geometry = new CombinedGeometry(GeometryCombineMode.Exclude, geometry, rect);
        }

        return geometry;
    }

    public static RenderTargetBitmap RenderImagesToBitmap(OverlayDataModel overlay)
    {
        return RenderImagesToBitmap(overlay.ImageModels, overlay.ClippingMaskModels, new PixelSize(overlay.Width, overlay.Height));
    }

    public static Bitmap GetPropertyReadyBitmap(ImageModel im)
    {
        if (!FileHandlerUtil.IsValidImagePath(im.ImagePath))
            throw new ArgumentException($"Invalid image path: {im.ImagePath}");

        var magickImage = ImageCache.GetMagickImage(im.ImagePath);

        return AddProperties(magickImage, im.ImageProperties);
    }

    public static Bitmap AddProperties(MagickImage image, ImagePropertiesModel properties)
    {
        if (properties.IsHMirrored)
        {
            image.Flop();
        }
        if (properties.IsVMirrored)
        {
            image.Flip();
        }
        if (properties.Scale != 1.0)
        {
            image.Scale((uint)properties.Width, (uint)properties.Height);
        }

        return image.ToWriteableBitmap();
    }
}

public static class ImageCache
{
    private static readonly ConcurrentDictionary<string, MagickImage> _cache = new();

    public static MagickImage GetMagickImage(string path)
    {
        if (_cache.TryGetValue(path, out var cached))
        {
            return (MagickImage)cached.Clone();
        }

        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            var image = new MagickImage(fs);
            _cache[path] = (MagickImage)image.Clone();
            return image;
        }
    }

    public static void ClearCache()
    {
        foreach (var kvp in _cache)
        {
            kvp.Value.Dispose();
        }
        _cache.Clear();
    }
}