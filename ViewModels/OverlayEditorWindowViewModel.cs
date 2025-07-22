using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using UltrawideOverlays.Enums;
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
        private bool _previewEnabled;

        [ObservableProperty]
        private int _previewSize;

        [ObservableProperty]
        private double _previewOpacity;

        [ObservableProperty]
        private string _previewColor;

        [ObservableProperty]
        private ImageModel? _selected;

        [ObservableProperty]
        private Boolean _propertiesEnabled;

        [ObservableProperty]
        private string? _overlayName;

        [ObservableProperty]
        private ObservableCollection<ClippingMaskModel> _maskTypes;

        [ObservableProperty]
        private ClippingMaskModel? _selectedMaskType;

        [ObservableProperty]
        private bool _canCreateOverlay;

        private readonly OverlayDataService _overlayDataService;
        private readonly SettingsDataService _settingsDataService;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        ///Design-only constructor
        public OverlayEditorWindowViewModel()
        {
            Images =
                [
                    new ImageModel("", "Example Image 1"),
                ];

            MaskTypes = GetMaskTypes();
        }

        ~OverlayEditorWindowViewModel()
        {
            Debug.WriteLine("OverlayEditorWindowViewModel finalized!");
        }

        public OverlayEditorWindowViewModel(OverlayDataService overlayService, SettingsDataService settingsService, Object? args = null)
        {
            _overlayDataService = overlayService;
            _settingsDataService = settingsService;

            Images = [];
            Images.CollectionChanged += ImagesCollectionChanged;

            if (args is OverlayDataModel existingModel)
            {
                OverlayName = existingModel.Name;
                for (int i = 0; i < existingModel.ImageModels.Count; i++)
                {
                    var image = existingModel.ImageModels[i];
                    Images.Add(image);
                }
                for (int i = 0; i < existingModel.ClippingMaskModels.Count; i++)
                {
                    var image = existingModel.ClippingMaskModels[i];
                    Images.Add(image);
                }
            }

            MaskTypes = GetMaskTypes();
            ConfigureGrid();
        }

        private async Task ConfigureGrid()
        {
            var settingsSize = await _settingsDataService.LoadSettingAsync(SettingsNames.GridSize);
            var settingsOpacity = await _settingsDataService.LoadSettingAsync(SettingsNames.GridOpacity);
            var settingsColor = await _settingsDataService.LoadSettingAsync(SettingsNames.GridColor);

            try
            {
                PreviewEnabled = true;
                PreviewSize = int.Parse(settingsSize);
                PreviewOpacity = double.Parse(settingsOpacity, System.Globalization.NumberStyles.Number);
                PreviewColor = settingsColor;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ConfigureGrid failed, exception:{ex.Message}");
            }
        }

        private ObservableCollection<ClippingMaskModel>? GetMaskTypes()
        {
            var output = new ObservableCollection<ClippingMaskModel>();
            for (int i = 0; i < (int)ClippingMaskType.Amount; i++)
            {
                var mask = ClippingMaskModel.GetMaskByType(0, 0, (ClippingMaskType)i);
                output.Add(mask);
            }

            return output;
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

        private void ImagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CanCreateOverlay = Images.Count > 0;
        }


        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////

        [RelayCommand]
        public void AddImageModel(IEnumerable<Uri> imageFilePaths)
        {
            foreach (var uri in imageFilePaths)
            {
                try
                {
                    var newImage = new ImageModel(uri.LocalPath, FileHandlerUtil.GetFileName(uri));
                    Images.Add(newImage);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to add image model from path {uri.LocalPath}: {ex.Message}");
                    continue;
                }
            }
            CanCreateOverlay = Images.Count > 0;
        }

        [RelayCommand]
        public void RemoveImageModel()
        {
            if (Selected != null)
            {
                Images.Remove(Selected);
                Selected = null;
            }
            CanCreateOverlay = Images.Count > 0;
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

            var overlay = new OverlayDataModel()
            {
                Name = name,
                ImageModels = new List<ImageModel>(),
                ClippingMaskModels = new List<ClippingMaskModel>(),
                Width = pixelSize.Width,
                Height = pixelSize.Height,
                LastModified = DateTime.Now,
                LastUsed = DateTime.Now
            };

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

            overlay.NumberOfImages = overlay.ImageModels.Count + overlay.ClippingMaskModels.Count;

            _ = _overlayDataService.SaveOverlayAsync(overlay);
        }

        [RelayCommand]
        public void AddClippingMask(PixelSize MonitorSize)
        {
            if (SelectedMaskType == null) return;

            var clippingMask = SelectedMaskType;
            clippingMask.ImageProperties.PositionX = (MonitorSize.Width - clippingMask.ImageProperties.Width) / 2.0;
            clippingMask.ImageProperties.PositionY = (MonitorSize.Height - clippingMask.ImageProperties.Height) / 2.0;

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
