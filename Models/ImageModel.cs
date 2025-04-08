using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltrawideOverlays.Models
{
    public class ImageModel
    {
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public string ImageCategory { get; set; }
        public string ImageResolution { get; set; }
        public string ImageFormat { get; set; }

        public ImageModel(string imagePath, string imageName, string imageDescription, string imageCategory, string imageResolution, string imageFormat)
        {
            ImagePath = imagePath;
            ImageName = imageName;
            ImageCategory = imageCategory;
            ImageResolution = imageResolution;
            ImageFormat = imageFormat;
        }
    }
}
