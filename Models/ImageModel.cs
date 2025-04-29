using CommunityToolkit.Mvvm.ComponentModel;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Models
{
    public partial class ImageModel : ObservableObject
    {
        public string ImagePath { get; set; }
        public string ImageName { get; set; }

        [ObservableProperty]
        private ImagePropertiesModel _imageProperties;

        public ImageModel(string imagePath, string imageName)
        {
            ImagePath = imagePath;
            ImageName = imageName;
            _imageProperties = new ImagePropertiesModel();

            //TODO: Is there not a better way to get the ImageWidth and Height
            using var bitmap = new Avalonia.Media.Imaging.Bitmap(imagePath);
            ImageProperties.Width = bitmap.PixelSize.Width;
            ImageProperties.Height = bitmap.PixelSize.Height;
        }
    }
}
