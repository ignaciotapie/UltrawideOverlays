using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using System.ComponentModel;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Models;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.CustomControls
{
    public class SelectableImage : SelectableItemBase
    {
        public readonly ImageModel? ImageModel;
        public Image? image;
        public Border? border;
        private bool disposedValue;

        public SelectableImage(ImageModel im)
        {
            ImageModel = im;
            DataContext = im;
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
                Stretch = Stretch.Fill //TODO: Add stretch options
            };
            image.Bind(Image.SourceProperty, new Binding("ImagePath")
            {
                Converter = PathToCachedBitmapConverter.Instance,
                Mode = BindingMode.OneWay
            });

            image.Bind(Image.WidthProperty, new Binding("ImageProperties.Width", BindingMode.TwoWay));
            image.Bind(Image.HeightProperty, new Binding("ImageProperties.Height", BindingMode.TwoWay));
            image.Bind(Image.OpacityProperty, new Binding("ImageProperties.Opacity", BindingMode.TwoWay));
            image.Bind(Image.IsVisibleProperty, new Binding("ImageProperties.IsVisible", BindingMode.TwoWay));

            Bind(ToolTip.TipProperty, new Binding("ImageName"));
            Bind(DragPanel.DraggableProperty, new Binding("ImageProperties.IsDraggable", BindingMode.TwoWay));
            Bind(DragPanel.PositionProperty, new Binding("ImageProperties.Position", BindingMode.TwoWay));

            border = new Border
            {
                Child = image,
                IsHitTestVisible = false,
                Background = Brushes.Transparent,
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(0),
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            Child = border;
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
        private void UpdateSelectionVisual()
        {
            if (border == null) return;
            if (IsSelected == true)
            {
                border.BoxShadow = new BoxShadows(
                    new BoxShadow
                    {
                        Color = Colors.DodgerBlue,
                        Blur = 5,
                        Spread = 3,
                        OffsetX = 0,
                        OffsetY = 0
                    });
            }
            else
            {
                border.BoxShadow = new BoxShadows(
                    new BoxShadow
                    {
                        Color = Colors.Transparent,
                        Blur = 0,
                        Spread = 0,
                        OffsetX = 0,
                        OffsetY = 0
                    });
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
            return LayoutHelper.MeasureChild(Child, availableSize, new Thickness(0), new Thickness(0));
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            return LayoutHelper.ArrangeChild(Child, finalSize, new Thickness(0), new Thickness(0));
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

        public new void Dispose()
        {
            if (!disposedValue)
            {
                if (ImageModel?.ImageProperties is ImagePropertiesModel props)
                    props.PropertyChanged -= ImageModelPropertiesPropertyChange;

                if (ImageModel != null)
                    ImageModel.PropertyChanged -= ImageModelPropertyChange;

                image = null;
                border = null;
                Child = null;

                ClearValue(ToolTip.TipProperty);

                disposedValue = true;
            }
            base.Dispose();
        }
    }
}
