using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Models
{
    public partial class ImageModel : ModelBase
    {
        public string ImagePath { get; set; }

        [ObservableProperty]
        public string _imageName;

        [ObservableProperty]
        public bool _isClippingMask = false;

        [ObservableProperty]
        private ImagePropertiesModel _imageProperties;

        public ImageModel()
        {
            ImagePath = string.Empty;
            ImageName = string.Empty;
            ImageProperties = new ImagePropertiesModel();
        }

        public ImageModel(string imagePath, string imageName, ImagePropertiesModel? imageProperties = null)
        {
            ImagePath = imagePath;
            ImageName = imageName;
            ImageProperties = imageProperties ?? new ImagePropertiesModel();

            if (imageProperties != null) return;

            if (FileHandlerUtil.IsValidImagePath(imagePath) == false)
            {
                //Finish Image Model here, handle empty paths later
                return;
            }

            using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var image = new Bitmap(fileStream))
                {
                    ImageProperties.Width = image.PixelSize.Width;
                    ImageProperties.Height = image.PixelSize.Height;
                    ImageProperties.OriginalWidth = image.PixelSize.Width;
                    ImageProperties.OriginalHeight = image.PixelSize.Height;
                }
            }
        }

        public override string ToString()
        {
            return $"ImageModel: {ImageName} ({ImagePath}) - Properties: {ImageProperties.ToString()}";
        }

        public override ImageModel Clone()
        {
            return new ImageModel
            {
                ImagePath = this.ImagePath,
                ImageName = this.ImageName,
                IsClippingMask = this.IsClippingMask,
                ImageProperties = ImageProperties.Clone()
            };
        }
    }
}
