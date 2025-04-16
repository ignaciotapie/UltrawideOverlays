namespace UltrawideOverlays.Models
{
    public class ImageModel
    {
        public string ImagePath { get; set; }
        public string ImageName { get; set; }

        public ImageModel(string imagePath, string imageName)
        {
            ImagePath = imagePath;
            ImageName = imageName;
        }
    }
}
