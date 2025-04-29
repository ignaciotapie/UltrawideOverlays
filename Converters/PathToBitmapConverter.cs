using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Converters
{
    static public class PathToBitmapConverter
    {
        /// <summary>
        /// Gets a Converter that takes a number as input and converts it into a text representation
        /// </summary>
        public static FuncValueConverter<string?, Bitmap> Converter { get; } =
            new FuncValueConverter<string?, Bitmap>(filePath =>
            {
                if (filePath == null)
                    return null;

                // Check if the path is a valid file path
                if (FileHandlerUtil.IsValidImagePath(filePath))
                {
                    // Use the BitmapLoader to load the image
                    return new Bitmap(filePath);
                }
                return null;
            });
    }
}
