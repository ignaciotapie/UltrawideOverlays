using Avalonia;
using Avalonia.Media.Imaging;
using System.Collections.Generic;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.Utils;

public static class ImageRenderer
{
    public static RenderTargetBitmap RenderImagesToBitmap(IEnumerable<ImageModel> imageModels, PixelSize outputSize)
    {
        var bitmap = new RenderTargetBitmap(outputSize);

        using (var ctx = bitmap.CreateDrawingContext())
        {
            foreach (var im in imageModels)
            {
                var props = im.ImageProperties;

                if (!props.IsVisible || !FileHandlerUtil.IsValidImagePath(im.ImagePath))
                    continue;

                using var image = new Bitmap(im.ImagePath);
                var hMirror = props.IsHMirrored ? -1 : 1;
                var vMirror = props.IsVMirrored ? -1 : 1;

                var transform = Matrix.CreateTranslation(-props.Width / 2, -props.Height / 2) *
                                Matrix.CreateScale(hMirror * props.Scale, vMirror * props.Scale) *
                                Matrix.CreateTranslation(props.PositionX + props.Width / 2, props.PositionY + props.Height / 2);

                var disposableTransformation = ctx.PushTransform(transform);
                ctx.DrawImage(image, new Rect(0, 0, props.Width, props.Height));
                disposableTransformation.Dispose();
            }
        }

        return bitmap;
    }

    public static RenderTargetBitmap RenderImagesToBitmap(OverlayDataModel overlay)
    {
        return RenderImagesToBitmap(overlay.ImageModels, new PixelSize(overlay.Width, overlay.Height));
    }
}