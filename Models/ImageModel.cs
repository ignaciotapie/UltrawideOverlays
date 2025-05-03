using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrawideOverlays.Models
{
    public partial class ImageModel : ModelBase
    {
        public string ImagePath { get; set; }

        [ObservableProperty]
        public string _imageName;

        [ObservableProperty]
        private ImagePropertiesModel _imageProperties;

        public ImageModel(string imagePath, string imageName, ImagePropertiesModel? imageProperties = null)
        {
            ImagePath = imagePath;
            ImageName = imageName;
            ImageProperties = imageProperties ?? new ImagePropertiesModel();

            //TODO: Is there not a better way to get the ImageWidth and Height
            using var bitmap = new Avalonia.Media.Imaging.Bitmap(imagePath);
            ImageProperties.Width = bitmap.PixelSize.Width;
            ImageProperties.Height = bitmap.PixelSize.Height;
        }

        public override ImageModel Clone()
        {
            return new ImageModel(ImagePath, ImageName, ImageProperties.Clone());
        }
    }
}
