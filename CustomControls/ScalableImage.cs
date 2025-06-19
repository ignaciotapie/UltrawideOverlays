using Avalonia;
using Avalonia.Controls;
using System;

namespace UltrawideOverlays.CustomControls
{
    public class ScalableImage : Image
    {
        public ScalableImage()
        {
            Stretch = Avalonia.Media.Stretch.Uniform;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var desiredSize = base.MeasureOverride(availableSize);
            return new Size(Math.Min(desiredSize.Width, availableSize.Width), Math.Min(desiredSize.Height, availableSize.Height));
        }
    }
}
