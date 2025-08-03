using Avalonia;
using Avalonia.Media;

namespace UltrawideOverlays.Models
{
    public enum ClippingMaskType
    {
        Default,
        FourEightyP,
        SevenTwentyP,
        TenEightyP,
        FourteenFortyP,
        TwentyOneSixtyP,
        Amount
    }

    public class ClippingMaskModel : ImageModel
    {
        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        public ClippingMaskModel() : base("", "ClippingMask")
        {
            base.ImageProperties.Width = 0;
            base.ImageProperties.Height = 0;
            base.ImageProperties.Opacity = 0.3;
            base.ImageProperties.IsDraggable = false;
        }

        public ClippingMaskModel(Geometry geo) : base("", "ClippingMask")
        {
            base.ImageProperties.Width = (int)geo.Bounds.Width;
            base.ImageProperties.Height = (int)geo.Bounds.Height;
            base.ImageProperties.OriginalHeight = (int)geo.Bounds.Height;
            base.ImageProperties.OriginalWidth = (int)geo.Bounds.Width;
            base.ImageProperties.Opacity = 0.3;
            base.ImageProperties.IsDraggable = false;
        }

        public static ClippingMaskModel GetMaskByType(double positionX = 0, double positionY = 0, ClippingMaskType type = ClippingMaskType.Default)
        {
            ClippingMaskModel model;
            switch (type)
            {
                case ClippingMaskType.FourEightyP:
                    model = new ClippingMaskModel(new RectangleGeometry(new Rect(positionX, positionY, 640, 480)));
                    model.ImageName = "480p Mask";
                    break;
                case ClippingMaskType.SevenTwentyP:
                    model = new ClippingMaskModel(new RectangleGeometry(new Rect(positionX, positionY, 1280, 720)));
                    model.ImageName = "720p Mask";
                    break;
                case ClippingMaskType.TenEightyP:
                    model = new ClippingMaskModel(new RectangleGeometry(new Rect(positionX, positionY, 1920, 1080)));
                    model.ImageName = "1080p Mask";
                    break;
                case ClippingMaskType.FourteenFortyP:
                    model = new ClippingMaskModel(new RectangleGeometry(new Rect(positionX, positionY, 2560, 1440)));
                    model.ImageName = "1440p Mask";
                    break;
                case ClippingMaskType.TwentyOneSixtyP:
                    model = new ClippingMaskModel(new RectangleGeometry(new Rect(positionX, positionY, 3840, 2160)));
                    model.ImageName = "2160p Mask";
                    break;
                case ClippingMaskType.Default:
                default:
                    model = new ClippingMaskModel(new RectangleGeometry(new Rect(positionX, positionY, 800, 800)));
                    model.ImageName = "Default Mask";
                    break;
            }
            return model;
        }
    }
}
