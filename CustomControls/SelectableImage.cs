using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.ComponentModel;
using System.Diagnostics;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;
using UltrawideOverlays.Wrappers;

namespace UltrawideOverlays.CustomControls
{
    public class SelectableImage : SelectableItemBase, IDisposable
    {
        public readonly ImageModel? ImageModel;
        public Image? image;
        private bool disposedValue;

        private static readonly BoxShadows boxShadow = new BoxShadows(new BoxShadow
        {
            Color = Colors.DodgerBlue,
            Blur = 5,
            Spread = 3,
            OffsetX = 0,
            OffsetY = 0
        });
        private static readonly BoxShadows invisibleShadow = new BoxShadows(new BoxShadow
        {
            Color = Colors.Transparent,
            Blur = 0,
            Spread = 0,
            OffsetX = 0,
            OffsetY = 0
        });

        public SelectableImage(ImageWrapper wrapper)
        {
            ImageModel = wrapper.Model as ImageModel;
            DataContext = wrapper;
            Background = Brushes.Transparent;
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
            RenderTransformOrigin = RelativePoint.TopLeft;
            image = new Image
            {
                IsHitTestVisible = false,
                Focusable = false,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Stretch = Stretch.Fill
            };
            image.Bind(Image.SourceProperty, new Binding("ImageSource")
            {
                Converter = new NullToImageConverter()
            });

            image.Bind(Image.WidthProperty, new Binding("Model.ImageProperties.Width", BindingMode.TwoWay));
            image.Bind(Image.HeightProperty, new Binding("Model.ImageProperties.Height", BindingMode.TwoWay));
            image.Bind(Image.OpacityProperty, new Binding("Model.ImageProperties.Opacity", BindingMode.TwoWay));
            image.Bind(Image.IsVisibleProperty, new Binding("Model.ImageProperties.IsVisible", BindingMode.TwoWay));

            Bind(ToolTip.TipProperty, new Binding("Model.ImageName"));
            Bind(DragPanel.DraggableProperty, new Binding("Model.ImageProperties.IsDraggable", BindingMode.TwoWay));
            Bind(DragPanel.PositionProperty, new Binding("Model.ImageProperties.Position", BindingMode.TwoWay));
            Bind(ZIndexProperty, new Binding("Model.ImageProperties.ZIndex", BindingMode.TwoWay));

            Child = image;
        }


        ///////////////////////////////////////////
        /// HANDLERS
        ///////////////////////////////////////////
        private void ImageModelPropertiesPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"ImageModelProperties Changed, properties: {sender.ToString()}");
            if (e.PropertyName == nameof(ImagePropertiesModel.IsHMirrored) ||
                e.PropertyName == nameof(ImagePropertiesModel.IsVMirrored) ||
                e.PropertyName == nameof(ImagePropertiesModel.Scale))
            {
                UpdateTransform();
            }
            if (e.PropertyName == nameof(ImagePropertiesModel.Width) ||
                e.PropertyName == nameof(ImagePropertiesModel.Height))
            {
                InvalidateMeasure();
            }
        }

        private void ImageModelPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"ImageModel Changed, properties: {sender.ToString()}");
            if (e.PropertyName == nameof(ImageModel.ImagePath) ||
                e.PropertyName == nameof(ImageModel.ImageName))
            {
                RefreshBitmap();
            }
        }
        private void UpdateSelectionVisual()
        {
            BoxShadow = IsSelected ? boxShadow : invisibleShadow;
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
            UpdateSelectionVisual();

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
            return LayoutHelper.MeasureChild(Child, new Size(ImageModel.ImageProperties.Width, ImageModel.ImageProperties.Height), new Thickness(0), new Thickness(0));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return LayoutHelper.ArrangeChild(Child, new Size(ImageModel.ImageProperties.Width, ImageModel.ImageProperties.Height), new Thickness(0), new Thickness(0));
        }

        protected override void OnItemSelectedChanged(bool isSelected)
        {
            UpdateSelectionVisual();
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

        public void Dispose()
        {
            if (!disposedValue)
            {
                if (ImageModel?.ImageProperties is ImagePropertiesModel props)
                    props.PropertyChanged -= ImageModelPropertiesPropertyChange;

                if (ImageModel != null)
                    ImageModel.PropertyChanged -= ImageModelPropertyChange;

                image = null;
                Child = null;

                ClearValue(ToolTip.TipProperty);

                disposedValue = true;
                (DataContext as ImageWrapper)?.Dispose();
                DataContext = null;
            }
        }
    }
}
