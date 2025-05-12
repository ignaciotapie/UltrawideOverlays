using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace UltrawideOverlays.Utils
{
    public sealed class ImageExtension
    {
        public static readonly ImageExtension PNG = new ImageExtension(".png");
        public static readonly ImageExtension JPG = new ImageExtension(".jpg");
        public static readonly ImageExtension JPEG = new ImageExtension(".jpeg");
        public static readonly ImageExtension BMP = new ImageExtension(".bmp");

        public string Extension { get; }

        private ImageExtension(string extension)
        {
            Extension = extension;
        }

        public override string ToString() => Extension;

        public static implicit operator string(ImageExtension imageExtension)
        {
            return imageExtension.Extension;
        }
    }

    public static class FileHandlerUtil
    {
        private static readonly HashSet<string> _validImageExtensions = [ImageExtension.PNG, ImageExtension.BMP, ImageExtension.JPG, ImageExtension.JPEG];

        /// <summary>
        /// Returns file name without extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>string FileName</returns>
        public static string GetFileName(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        public static string GetFileName(Uri uri)
        {
            return Path.GetFileNameWithoutExtension(uri.ToString());
        }

        /// <summary>
        /// Returns file extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath);
        }

        /// <summary>
        /// Returns file name with extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>string FileName.Extension</returns>
        public static string GetFileNameWithExtension(string filePath)
        {
            return Path.GetFileName(filePath);
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

        public static string? AddImageFileExtension(string filePath, ImageExtension fileExtension)
        {
            if (_validImageExtensions.Contains(fileExtension))
            {
                return filePath + fileExtension;
            }

            return null;
        }

        public static string AddJSONFileExtension(string filePath) 
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
                var dirInfo = new DirectoryInfo(path);
                return dirInfo.Exists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsValidFilePath(string path) 
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var fileInfo = new FileInfo(path);
                return fileInfo.Exists;
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
                    Directory.CreateDirectory(folderPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating directory: {ex.Message}");
                }
            }
        }

        public static async Task WriteToJSON(string path, string json)
        {
            path = AddJSONFileExtension(path);
            await File.WriteAllTextAsync(path, json);
        }

        public static async Task<string> ReadFromJSON(string path)
        {
            path = AddJSONFileExtension(path);
            return await File.ReadAllTextAsync(path);
        }
    }
}
