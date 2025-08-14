using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Utils;

public static class ImageRenderer
{
    public static RenderTargetBitmap RenderImagesToBitmap(IEnumerable<ImageModel> imageModels, IEnumerable<ImageModel> clippingMaskModels, PixelSize outputSize)
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

                var image = new Bitmap(im.ImagePath);

                using (ctx.PushTransform(GetTransformMatrix(props)))
                {
                    //For some reason the transform affects THE ENTIRE DRAWING CONTEXT (should've realized that sooner)
                    //I THOUGHT IT ONLY AFFECTED THE IMAGES ABOUT TO BE DRAWN T_T!!
                    //What does this mean? Grid coordinates are opposite if mirrored.
                    //It kinda makes sense, but at the same time it doesn't, fml
                    var positionX = props.PositionX * (props.IsHMirrored ? -1 : 1);
                    var positionY = props.PositionY * (props.IsVMirrored ? -1 : 1);

                    // Apply opacity
                    using (ctx.PushOpacity(props.Opacity))
                    {
                        ctx.DrawImage(image, new Rect(positionX, positionY, props.Width, props.Height));
                    }
                }

                image.Dispose();
            }
        }
        return bitmap;
    }

    public static Matrix GetTransformMatrix(ImagePropertiesModel props)
    {
        // Start with identity matrix
        var matrix = Matrix.Identity;

        // Apply mirroring transformations
        if (props.IsHMirrored)
        {
            matrix = Matrix.CreateScale(-1, 1) * matrix;
        }
        if (props.IsVMirrored)
        {
            matrix = Matrix.CreateScale(1, -1) * matrix;
        }

        //Account for Rendering Origin being top-left
        if (props.IsHMirrored)
        {
            matrix = Matrix.CreateTranslation(-props.OriginalWidth * props.Scale, 0) * matrix;
        }
        if (props.IsVMirrored)
        {
            matrix = Matrix.CreateTranslation(0, -props.OriginalHeight * props.Scale) * matrix;
        }

        return matrix;
    }

    private static Geometry CreateGeometryFromClippingMasks(IEnumerable<ImageModel> masks, PixelSize outputSize)
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
}