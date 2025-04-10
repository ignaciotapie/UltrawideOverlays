using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltrawideOverlays.Utils
{
    public static class FileHandlerUtil
    {
        private static readonly string[] _validImageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".webp"];

        /// <summary>
        /// Returns file name without extension.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>string FileName</returns>
        public static string GetFileName(string filePath) 
        {
            return System.IO.Path.GetFileNameWithoutExtension(filePath);
        }

        internal static string GetFileName(Uri uri)
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
        public static bool IsValidImage(string filePath)
        {
            string fileExtension = GetFileExtension(filePath).ToLower();

            return _validImageExtensions.Contains(fileExtension);
        }

        public static bool IsValidImage(Uri uri)
        {
            string fileExtension = GetFileExtension(uri.ToString()).ToLower();

            return _validImageExtensions.Contains(fileExtension);
        }
    }
}
