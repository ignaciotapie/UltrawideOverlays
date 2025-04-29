using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayEditorWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ImageModel> _images;

        [ObservableProperty]
        private bool _previewEnabled = true;

        [ObservableProperty]
        private int _previewSize = 50;

        [ObservableProperty]
        private double _previewOpacity = 0.5;

        [ObservableProperty]
        private Color _previewColor = Colors.AliceBlue;

        [ObservableProperty]
        private ImageModel? _selected = null;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        public OverlayEditorWindowViewModel()
        {
            Images =
            [
                //Tried again and again to not use this hardcoded path, but it always throws an error... Ehhh. The real app will use absolute paths, so this is not a problem.
                
                new ImageModel("C:\\Users\\Nacho\\Downloads\\overlay.png", "overlay")
            ];

            PreviewEnabled = true;
            PreviewSize = 50;
            PreviewOpacity = 0.5;
            PreviewColor = Colors.AliceBlue;

            Selected = Images[0];
        }
        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////

        [RelayCommand]
        public void AddImageModel(IEnumerable<Uri> imageFilePaths)
        {
            foreach (var uri in imageFilePaths)
            {
                var newImage = new ImageModel(uri.AbsolutePath, FileHandlerUtil.GetFileName(uri));
                Images.Add(newImage);
            }
        }

        [RelayCommand]
        public void RemoveImageModel(ImageModel imageModel)
        {
            if (Images.Contains(imageModel))
            {
                Images.Remove(imageModel);
            }
        }

        [RelayCommand]
        public void SelectImage(ImageModel im)
        {
            Selected = im;
        }
    }
}
