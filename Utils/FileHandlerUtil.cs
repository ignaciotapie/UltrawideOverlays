using System;
using System.Collections.Generic;

namespace UltrawideOverlays.Utils
{
    public class ImageExtensions : Object
    {
        public static readonly string PNG = ".png";
        public static readonly string JPG = ".jpg";
        public static readonly string JPEG = ".jpeg";
        public static readonly string BMP = ".bmp";
    }

    public static class FileHandlerUtil
    {
        private static readonly HashSet<string> _validImageExtensions = [ImageExtensions.PNG, ImageExtensions.BMP, ImageExtensions.JPG, ImageExtensions.JPEG];

        /// <summary>
        /// Returns file name without extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>string FileName</returns>
        public static string GetFileName(string filePath)
        {
            return System.IO.Path.GetFileNameWithoutExtension(filePath);
        }

        public static string GetFileName(Uri uri)
        {
            return System.IO.Path.GetFileNameWithoutExtension(uri.ToString());
        }

        /// <summary>
        /// Returns file extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileExtension(string filePath)
        {
            return System.IO.Path.GetExtension(filePath);
        }

        /// <summary>
        /// Returns file name with extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>string FileName.Extension</returns>
        public static string GetFileNameWithExtension(string filePath)
        {
            return System.IO.Path.GetFileName(filePath);
        }

        /// <summary>
        /// Checks if the file is a valid image.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsValidImagePath(string filePath)
        {
            string fileExtension = GetFileExtension(filePath);

            return _validImageExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public static bool IsValidImagePath(Uri uri)
        {
            string fileExtension = GetFileExtension(uri.ToString());

            return _validImageExtensions.Contains(fileExtension.ToLowerInvariant());
        }

        public static string? AddImageFileExtension(string filePath, String fileExtension)
        {
            if (_validImageExtensions.Contains(fileExtension))
            {
                return filePath + fileExtension;
            }

            return null;
        }

        public static string AddJsonFileExtension(string filePath) 
        {
            const string jsonExtension = ".json";
            if (filePath.EndsWith(jsonExtension, StringComparison.OrdinalIgnoreCase))
            {
                return filePath;
            }

            return filePath + jsonExtension;
        }

        public static bool IsValidFolderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var dirInfo = new System.IO.DirectoryInfo(path);
                return dirInfo.Exists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void CreateDirectory(string folderPath)
        {
            if (!IsValidFolderPath(folderPath))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory: {ex.Message}");
                }
            }
        }
    }
}
