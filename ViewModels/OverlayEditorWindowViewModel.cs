using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
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
        private IBrush _previewColor = Brushes.AliceBlue;

        [ObservableProperty]
        private ImageModel? _selected = null;

        [ObservableProperty]
        private Boolean _propertiesEnabled = false;

        [ObservableProperty]
        private string? _overlayName = null;

        private readonly OverlayDataService _overlayDataService;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        ///Design-only constructor
        public OverlayEditorWindowViewModel()
        {
            Images = [];
        }

        public OverlayEditorWindowViewModel(OverlayDataService service, Object? args)
        {
            _overlayDataService = service;

            if (args is OverlayDataModel existingModel)
            {
                OverlayName = existingModel.Name;
                Images = new ObservableCollection<ImageModel>(existingModel.ImageModels);
                foreach (var clippingMask in existingModel.ClippingMaskModels)
                {
                    Images.Add(clippingMask);
                }
            }
            else
            {
                Images = [];
            }
        }

        partial void OnSelectedChanged(ImageModel? oldValue, ImageModel? newValue)
        {
            Debug.WriteLine($"Image selected changed from {oldValue?.ImageName} to {newValue?.ImageName}");
            if (newValue != null)
            {
                PropertiesEnabled = true;
            }
            else
            {
                PropertiesEnabled = false;
            }
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
        public void RemoveImageModel()
        {
            if (Selected != null)
            {
                Images.Remove(Selected);
                Selected = null;
            }
        }

        [RelayCommand]
        public void SelectImage(ImageModel im)
        {
            Selected = im;
        }

        [RelayCommand]
        public void DuplicateImageModel()
        {
            if (Selected != null)
            {
                var newImage = Selected.Clone();
                Images.Add(newImage);
            }
        }

        [RelayCommand]
        public void CreateOverlay(PixelSize pixelSize)
        {
            String? name = (OverlayName != null) ? OverlayName : "Overlay";

            var overlay = new OverlayDataModel();
            overlay.Name = name;

            overlay.ImageModels = new List<ImageModel>();
            overlay.ClippingMaskModels = new List<ClippingMaskModel>();
            for (int i = 0; i < Images.Count; i++)
            {
                if (Images[i] is ClippingMaskModel clippingMask)
                {
                    overlay.ClippingMaskModels.Add(clippingMask);
                }
                else
                {
                    overlay.ImageModels.Add(Images[i]);
                }
            }

            overlay.Width = pixelSize.Width;
            overlay.Height = pixelSize.Height;
            overlay.NumberOfImages = overlay.ImageModels.Count;
            overlay.LastModified = DateTime.Now;
            overlay.LastUsed = DateTime.Now;

            _ = _overlayDataService.SaveOverlayAsync(overlay);
        }

        [RelayCommand]
        public void AddClippingMask()
        {
            //TODO default?
            var clippingMask = new ClippingMaskModel(new RectangleGeometry(new Rect(0, 0, 800, 800)));
            Images.Add(clippingMask);
        }

        [RelayCommand]
        public void MirrorPositionX(PixelSize bounds)
        {
            if (Selected == null) return;

            double centerX = bounds.Width / 2.0;
            double imageCenterX = Selected.ImageProperties.PositionX + Selected.ImageProperties.Width / 2.0;

            // Distance from image center to canvas center
            double distanceFromCenter = imageCenterX - centerX;

            // Mirror the center and reposition the top-left corner accordingly
            double newCenterX = centerX - distanceFromCenter;
            double newPositionX = newCenterX - Selected.ImageProperties.Width / 2.0;

            Selected.ImageProperties.Position = new Point(newPositionX, Selected.ImageProperties.PositionY);
        }

        [RelayCommand]
        public void MirrorPositionY(PixelSize bounds)
        {
            if (Selected == null) return;

            double centerY = bounds.Height / 2.0;
            double imageCenterY = Selected.ImageProperties.PositionY + Selected.ImageProperties.Height / 2.0;

            // Distance from image center to canvas center
            double distanceFromCenter = imageCenterY - centerY;

            // Mirror the center and reposition the top-left corner accordingly
            double newCenterY = centerY - distanceFromCenter;
            double newPositionY = newCenterY - Selected.ImageProperties.Height / 2.0;

            Selected.ImageProperties.Position = new Point(Selected.ImageProperties.PositionX, newPositionY);
        }
    }
}
