using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using UltrawideOverlays.Decorator;
using UltrawideOverlays.Enums;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
using UltrawideOverlays.Utils;
using UltrawideOverlays.Wrappers;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayEditorWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ImageWrapper> _images;

        [ObservableProperty]
        private bool _previewEnabled;

        [ObservableProperty]
        private int _previewSize;

        [ObservableProperty]
        private double _previewOpacity;

        [ObservableProperty]
        private string _previewColor;

        [ObservableProperty]
        private ImageWrapper? _selected;

        [ObservableProperty]
        private Boolean _propertiesEnabled;

        [ObservableProperty]
        private string? _overlayName;

        [ObservableProperty]
        private ObservableCollection<ImageWrapper> _maskTypes;

        [ObservableProperty]
        private ImageWrapper? _selectedMaskType;

        [ObservableProperty]
        private bool _canCreateOverlay;

        private OverlayDataService? _overlayDataService;
        private SettingsDataService? _settingsDataService;
        private ImageWrapperDecorator? _wrapperDecorator;
        private bool isDisposed = false;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        ///Design-only constructor
        public OverlayEditorWindowViewModel()
        {
        }

        public OverlayEditorWindowViewModel(OverlayDataService overlayService, SettingsDataService settingsService, ImageWrapperDecorator wrapperDecorator, Object? args = null)
        {
            _overlayDataService = overlayService;
            _settingsDataService = settingsService;
            _wrapperDecorator = wrapperDecorator;

            Images = [];
            Images.CollectionChanged += ImagesCollectionChanged;

            if (args is OverlayDataModel existingModel)
            {
                LoadExistingOverlay(existingModel);
            }

            MaskTypes = GetMaskTypes();
            ConfigureGrid();
        }

        ~OverlayEditorWindowViewModel()
        {
            Dispose();
        }

        private void LoadExistingOverlay(OverlayDataModel existingModel)
        {
            OverlayName = existingModel.Name;
            for (int i = 0; i < existingModel?.ImageModels?.Count; i++)
            {
                var image = existingModel.ImageModels[i].Clone();
                Images.Add(_wrapperDecorator.CreateImageWrapper(image, image.ImagePath));
            }
            for (int i = 0; i < existingModel?.ClippingMaskModels?.Count; i++)
            {
                var image = existingModel.ClippingMaskModels[i].Clone();
                Images.Add(_wrapperDecorator.CreateImageWrapper(image, image.ImagePath));
            }
        }

        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////
        partial void OnSelectedChanged(ImageWrapper? oldValue, ImageWrapper? newValue)
        {
            Debug.WriteLine($"Image selected changed from {oldValue?.ToString()} to {newValue?.ToString()}");
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
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

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

        private ObservableCollection<ImageWrapper>? GetMaskTypes()
        {
            var output = new ObservableCollection<ImageWrapper>();
            for (int i = 0; i < (int)ClippingMaskType.Amount; i++)
            {
                var mask = ClippingMaskModel.GetMaskByType(0, 0, (ClippingMaskType)i);
                output.Add(_wrapperDecorator.CreateImageWrapper(mask, mask.ImagePath));
            }

            return output;
        }

        private void ImagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CanCreateOverlay = Images.Count > 0;
            // Update ZIndex
            for (int i = 0; i < Images.Count; i++)
            {
                if (Images[i].Model is ImageModel imageModel)
                {
                    imageModel.ImageProperties.ZIndex = i;
                }
            }
        }

        private void MoveIndexOfImage(int currentIndex, int newIndex)
        {
            Images.Move(currentIndex, newIndex);

            Selected = Images[newIndex];
        }


        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////

        [RelayCommand]
        public void AddImageModel(IEnumerable<String> imageFilePaths)
        {
            foreach (var imagePath in imageFilePaths)
            {
                try
                {
                    var newImage = new ImageModel(imagePath, FileHandlerUtil.GetFileName(imagePath));
                    newImage.ImageProperties.ZIndex = Images.Count;
                    Images.Add(_wrapperDecorator.CreateImageWrapper(newImage, newImage.ImagePath));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to add image model from path {imagePath}: {ex.Message}");
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
                var wrapperToRemove = Selected;
                Selected = null;

                Images.Remove(wrapperToRemove);

                wrapperToRemove.Dispose();
            }
            CanCreateOverlay = Images.Count > 0;
        }

        [RelayCommand]
        public void DuplicateImageModel()
        {
            if (Selected != null)
            {
                var model = Selected.Model?.Clone();
                var newImage = _wrapperDecorator.CreateImageWrapper(model, model?.ImagePath);
                Images.Add(newImage);
                Selected = newImage;
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
                LastUsed = DateTime.Now,
                NumberOfImages = Images.Count
            };

            for (int i = 0; i < Images.Count; i++)
            {
                if (Images[i].Model is ClippingMaskModel clippingMask)
                {
                    overlay.ClippingMaskModels.Add(clippingMask);
                }
                else
                {
                    overlay.ImageModels.Add(Images[i].Model);
                }
            }

            _overlayDataService.SaveOverlayAsync(overlay);
        }

        [RelayCommand]
        public void AddClippingMask(PixelSize MonitorSize)
        {
            if (SelectedMaskType == null) return;

            var clippingMask = SelectedMaskType.Model;
            clippingMask.ImageProperties.PositionX = (MonitorSize.Width - clippingMask.ImageProperties.Width) / 2.0;
            clippingMask.ImageProperties.PositionY = (MonitorSize.Height - clippingMask.ImageProperties.Height) / 2.0;

            Images.Add(_wrapperDecorator.CreateImageWrapper(clippingMask, clippingMask.ImagePath));
        }

        [RelayCommand]
        public void MirrorPositionX(PixelSize bounds)
        {
            if (Selected == null) return;

            double centerX = bounds.Width / 2.0;
            var imageProperties = Selected.Model?.ImageProperties;
            if (imageProperties == null) return;
            double imageCenterX = imageProperties.PositionX + imageProperties.Width / 2.0;

            // Distance from image center to canvas center
            double distanceFromCenter = imageCenterX - centerX;

            // Mirror the center and reposition the top-left corner accordingly
            double newCenterX = centerX - distanceFromCenter;
            double newPositionX = newCenterX - imageProperties.Width / 2.0;

            imageProperties.Position = new Point(newPositionX, imageProperties.PositionY);
        }

        [RelayCommand]
        public void MirrorPositionY(PixelSize bounds)
        {
            if (Selected == null) return;

            double centerY = bounds.Height / 2.0;
            var imageProperties = Selected.Model?.ImageProperties;
            if (imageProperties == null) return;

            double imageCenterY = imageProperties.PositionY + imageProperties.Height / 2.0;

            // Distance from image center to canvas center
            double distanceFromCenter = imageCenterY - centerY;

            // Mirror the center and reposition the top-left corner accordingly
            double newCenterY = centerY - distanceFromCenter;
            double newPositionY = newCenterY - imageProperties.Height / 2.0;

            imageProperties.Position = new Point(imageProperties.PositionX, newPositionY);
        }

        [RelayCommand]
        public void MoveUpOrder(ImageWrapper? wrapper)
        {
            if (wrapper == null || Images.Count < 2 || !Images.Contains(wrapper))
                return;

            int currentIndex = Images.IndexOf(wrapper);
            if (currentIndex > 0)
            {
                MoveIndexOfImage(currentIndex, currentIndex - 1);
            }
        }

        [RelayCommand]
        public void MoveDownOrder(ImageWrapper? wrapper)
        {
            if (wrapper == null || Images.Count < 2 || !Images.Contains(wrapper))
                return;

            int currentIndex = Images.IndexOf(wrapper);
            if (currentIndex < Images.Count - 1)
            {
                MoveIndexOfImage(currentIndex, currentIndex + 1);
            }
        }

        [RelayCommand]
        public void MoveToTopOrder(ImageWrapper? wrapper)
        {
            if (wrapper == null || Images.Count < 2 || !Images.Contains(wrapper))
                return;

            int currentIndex = Images.IndexOf(wrapper);
            if (currentIndex > 0)
            {
                MoveIndexOfImage(currentIndex, 0);
            }
        }

        [RelayCommand]
        public void MoveToBottomOrder(ImageWrapper? wrapper)
        {
            if (wrapper == null || Images.Count < 2 || !Images.Contains(wrapper))
                return;

            int currentIndex = Images.IndexOf(wrapper);
            if (currentIndex < Images.Count - 1)
            {
                MoveIndexOfImage(currentIndex, Images.Count - 1);
            }
        }

        public override void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            Images.CollectionChanged -= ImagesCollectionChanged;
            foreach (var image in Images)
            {
                image.Dispose();
            }
            foreach (var mask in MaskTypes)
            {
                mask.Dispose();
            }
            Images.Clear();
            MaskTypes.Clear();

            Images = null;
            MaskTypes = null;
            OverlayName = null;
            PreviewColor = null;
            PreviewEnabled = false;
            PreviewSize = 0;
            PreviewOpacity = 0.0;
            PropertiesEnabled = false;
            CanCreateOverlay = false;

            Selected?.Dispose();
            Selected = null;

            SelectedMaskType?.Dispose();
            SelectedMaskType = null;

            _overlayDataService = null;
            _settingsDataService = null;
            _wrapperDecorator = null;

            Debug.WriteLine("OverlayEditorWindowViewModel disposed!");

            GC.SuppressFinalize(this);
        }
    }
}
