using Avalonia.Media;

namespace UltrawideOverlays.Models
{
    public class ClippingMaskModel : ImageModel
    {
        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        public ClippingMaskModel() : base(System.AppDomain.CurrentDomain.BaseDirectory + "Assets/Images/clipping-mask.png", "ClippingMask")
        {
            base.ImageProperties.Width = 0;
            base.ImageProperties.Height = 0;
            base.ImageProperties.Opacity = 0.3;
        }

        public ClippingMaskModel(Geometry geo) : base(System.AppDomain.CurrentDomain.BaseDirectory + "Assets/Images/clipping-mask.png", "ClippingMask")
        {
            base.ImageProperties.Width = (int)geo.Bounds.Width;
            base.ImageProperties.Height = (int)geo.Bounds.Height;
            base.ImageProperties.Opacity = 0.3;
        }
    }
}
