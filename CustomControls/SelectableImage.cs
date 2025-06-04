using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using ImageMagick;
using System;
using System.ComponentModel;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.CustomControls
{
    public class SelectableImage : SelectableItemBase
    {
        public ImageModel? ImageModel { get; private set; }
        public Image? image;
        private bool disposedValue;

        public SelectableImage(ImageModel im)
        {
            ImageModel = im;
            DataContext = im;
            Background = Brushes.Transparent;
            BorderBrush = null;
            BorderThickness = new Thickness(2);
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

            RenderTransformOrigin = RelativePoint.TopLeft;
            image = new Image
            {
                IsHitTestVisible = false,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Stretch = Stretch.Fill //TODO: Add stretch options
            };
            image.Bind(Image.WidthProperty, new Binding("ImageProperties.Width", BindingMode.TwoWay));
            image.Bind(Image.HeightProperty, new Binding("ImageProperties.Height", BindingMode.TwoWay));
            image.Bind(Image.OpacityProperty, new Binding("ImageProperties.Opacity", BindingMode.TwoWay));
            image.Bind(Image.IsVisibleProperty, new Binding("ImageProperties.IsVisible", BindingMode.TwoWay));

            Bind(IsHitTestVisibleProperty, new Binding("ImageProperties.IsDraggable", BindingMode.TwoWay));
            Bind(DragPanel.PositionProperty, new Binding("ImageProperties.Position", BindingMode.TwoWay));

            Child = image;
        }


        ///////////////////////////////////////////
        /// HANDLERS
        ///////////////////////////////////////////
        private void ImageModelPropertiesPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImagePropertiesModel.IsHMirrored) ||
                e.PropertyName == nameof(ImagePropertiesModel.IsVMirrored) ||
                e.PropertyName == nameof(ImagePropertiesModel.Scale))
            {
                UpdateTransform();
            }
        }

        private void ImageModelPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImageModel.ImagePath) || e.PropertyName == nameof(ImageModel.ImageName))
            {
                RefreshBitmap();
            }
        }

        private void RefreshBitmap()
        {
            if (ImageModel == null) return;

            if (image.Source is IDisposable disposable)
            {
                disposable.Dispose();
            }

            image.Source = ImageRenderer.GetPropertyReadyBitmap(ImageModel!);
            image.InvalidateVisual();
        }

        private void OnItemSelected(object? sender, object e)
        {
            UpdateBorder();
        }

        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            // Listen for mirror or scale changes
            if (ImageModel?.ImageProperties is ImagePropertiesModel props)
            {
                props.PropertyChanged += ImageModelPropertiesPropertyChange;
                UpdateTransform(); //Initial transform update
            }
            ImageModel.PropertyChanged += ImageModelPropertyChange;

            ItemSelectedChanged += OnItemSelected;
            base.OnAttachedToVisualTree(e);
        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            // Listen for mirror or scale changes
            if (ImageModel?.ImageProperties is ImagePropertiesModel props)
            {
                props.PropertyChanged -= ImageModelPropertiesPropertyChange;
            }
            ImageModel.PropertyChanged -= ImageModelPropertyChange;
            ItemSelectedChanged -= OnItemSelected;
            base.OnDetachedFromVisualTree(e);
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////
        private void UpdateBorder()
        {
            BorderBrush = IsSelected ? Brushes.DeepSkyBlue : null;
        }

        private void UpdateTransform()
        {
            RefreshBitmap();
        }

        public new void Dispose()
        {
            if (!disposedValue)
            {
                if (ImageModel?.ImageProperties is ImagePropertiesModel props)
                    props.PropertyChanged -= ImageModelPropertiesPropertyChange;

                if (ImageModel != null)
                    ImageModel.PropertyChanged -= ImageModelPropertyChange;

                ImageModel = null;
                image = null;
                disposedValue = true;
            }
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
