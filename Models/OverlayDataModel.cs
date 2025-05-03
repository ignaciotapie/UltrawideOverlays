using System.Collections.Generic;

namespace UltrawideOverlays.Models
{
    public class OverlayDataModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public IList<ImageModel>? ImageModels { get; set; }

        public OverlayDataModel()
        {
            Name = string.Empty;
            Path = string.Empty;
            ImageModels = new List<ImageModel>();
        }

        public OverlayDataModel(string overlayName, string overlayPath, IList<ImageModel>? imageModels = null)
        {
            Name = overlayName;
            Path = overlayPath;
            ImageModels = imageModels ?? new List<ImageModel>();
        }
    }
}
