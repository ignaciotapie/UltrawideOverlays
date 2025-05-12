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

        [ObservableProperty]
        private string? _overlayName = null;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        ///Design-only constructor
        public OverlayEditorWindowViewModel()
        {
            Images = [];
        }

        public OverlayEditorWindowViewModel(IList<ImageModel>? images = null)
        {
            Images = new ObservableCollection<ImageModel>(images ?? new List<ImageModel>());
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
            Images.Remove(imageModel);
        }

        [RelayCommand]
        public void SelectImage(ImageModel im)
        {
            Selected = im;
        }

        [RelayCommand]
        public void DuplicateImageModel(ImageModel imageModel)
        {
            if (imageModel != null)
            {
                var newImage = imageModel.Clone();
                Images.Add(newImage);
            }
        }

        [RelayCommand]
        public void CreateOverlay(PixelSize pixelSize)
        {
            var bitmap = Utils.ImageRenderer.RenderImagesToBitmap(Images, pixelSize);
            String? name = (OverlayName != null) ? OverlayName : "Overlay";
        }
    }
}
