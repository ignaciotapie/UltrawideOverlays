using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayEditorWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        ObservableCollection<ImageModel> _images;

        public OverlayEditorWindowViewModel()
        {
            Images = 
            [
                //Tried again and again to not use this hardcoded path, but it always throws an error... Ehhh. The real app will use absolute paths, so this is not a problem.
                new ImageModel("C:\\Users\\Nacho\\source\\repos\\UltrawideOverlays\\Assets\\Images\\test.png", "test"),
            ];
        }

        [RelayCommand]
        public void AddImage(IEnumerable<Uri> imageFilePaths)
        {
            foreach (var uri in imageFilePaths)
            {
                var newImage = new ImageModel(uri.AbsolutePath, FileHandlerUtil.GetFileName(uri));
                Images.Add(newImage);
            }
        }
    }
}
