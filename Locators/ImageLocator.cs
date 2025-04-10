using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using UltrawideOverlays.Models;
using UltrawideOverlays.ViewModels;

namespace UltrawideOverlays.Locators
{
    public class ImageLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            Canvas canvas = new Canvas();
            if (param is IEnumerable<ImageModel> images)
            {
                foreach (var image in images)
                {
                    var imageControl = new Image
                    {
                        Source = new Bitmap(image.ImagePath),
                        Width = 100,
                        Height = 100
                    };
                    canvas.Children.Add(imageControl);
                }
            }
            else
            {
                throw new ArgumentException("Invalid data type");
            }
            return canvas;
        }

        public bool Match(object? data)
        {
            return data is IEnumerable<ImageModel>;
        }
    }
}
