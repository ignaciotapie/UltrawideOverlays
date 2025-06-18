using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
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
                Focusable = false,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Stretch = Stretch.Fill //TODO: Add stretch options
            };
            image.Bind(Image.SourceProperty, new Binding("ImagePath")
            {
                Converter = new PathToBitmapConverter(),
                Mode = BindingMode.OneWay
            });

            image.Bind(Image.WidthProperty, new Binding("ImageProperties.Width", BindingMode.TwoWay));
            image.Bind(Image.HeightProperty, new Binding("ImageProperties.Height", BindingMode.TwoWay));
            image.Bind(Image.OpacityProperty, new Binding("ImageProperties.Opacity", BindingMode.TwoWay));
            image.Bind(Image.IsVisibleProperty, new Binding("ImageProperties.IsVisible", BindingMode.TwoWay));

            Bind(ToolTip.TipProperty, new Binding("ImageName"));
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
            if (e.PropertyName == nameof(ImageModel.ImagePath) ||
                e.PropertyName == nameof(ImageModel.ImageName))
            {
                RefreshBitmap();
            }
        }

        private void RefreshBitmap()
        {
            if (ImageModel == null) return;
            image.InvalidateVisual();
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
            base.OnDetachedFromVisualTree(e);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return LayoutHelper.MeasureChild(Child, availableSize, new Thickness(0), new Thickness(0));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return LayoutHelper.ArrangeChild(Child, finalSize, new Thickness(0), new Thickness(0));
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////
        private void UpdateTransform()
        {
            var props = ImageModel?.ImageProperties;
            var mirroringMatrix = ImageRenderer.GetTransformMatrix(props);

            RenderTransform = new MatrixTransform(mirroringMatrix);
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
