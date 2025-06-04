using System.Collections.Generic;

namespace UltrawideOverlays.Models
{
    public class OverlayDataModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public IList<ImageModel>? ImageModels { get; set; }
        public IList<ClippingMaskModel>? ClippingMaskModels { get; set; }

        public OverlayDataModel()
        {
            Name = string.Empty;
            Path = string.Empty;
            ImageModels = new List<ImageModel>();
            ClippingMaskModels = new List<ClippingMaskModel>();
        }

        public OverlayDataModel(string overlayName, string overlayPath, IList<ImageModel>? imageModels = null, IList<ClippingMaskModel>? masks = null)
        {
            Name = overlayName;
            Path = overlayPath;
            ImageModels = imageModels ?? new List<ImageModel>();
            ClippingMaskModels = masks ?? new List<ClippingMaskModel>();
        }
    }
}
