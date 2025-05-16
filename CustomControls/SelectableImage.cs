using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using System;
using System.ComponentModel;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.CustomControls
{
    public class SelectableImage : SelectableItemBase
    {
        public ImageModel? imageModel { get; private set; }
        public Image? image;
        private bool disposedValue;

        public SelectableImage(ImageModel im)
        {
            imageModel = im;
            DataContext = im;
            Background = Brushes.Transparent;
            BorderBrush = null;
            BorderThickness = new Thickness(2);
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

            image = new Image
            {
                IsHitTestVisible = false,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Stretch = Stretch.Fill, //TODO: Add stretch options
            };
            image.Bind(Image.SourceProperty, new Binding("ImagePath") { Converter = PathToBitmapConverter.Converter });
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
        private void ImageModelPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImagePropertiesModel.IsHMirrored) ||
                e.PropertyName == nameof(ImagePropertiesModel.IsVMirrored) ||
                e.PropertyName == nameof(ImagePropertiesModel.Scale))
            {
                UpdateTransform();
            };
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
            if (imageModel?.ImageProperties is ImagePropertiesModel props)
            {
                props.PropertyChanged += ImageModelPropertyChange;
                UpdateTransform(); //Initial transform update
            }

            ItemSelectedChanged += OnItemSelected;
            base.OnAttachedToVisualTree(e);
        }
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            // Listen for mirror or scale changes
            if (imageModel?.ImageProperties is ImagePropertiesModel props)
            {
                props.PropertyChanged -= ImageModelPropertyChange;
            }

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
            if (imageModel?.ImageProperties is not ImagePropertiesModel props)
                return;

            double scaleX = props.Scale;
            double scaleY = props.Scale;

            if (props.IsHMirrored) scaleX *= -1; //Flips horizontally
            if (props.IsVMirrored) scaleY *= -1; //Flips vertically

            this.RenderTransform = new ScaleTransform
            {
                ScaleX = scaleX,
                ScaleY = scaleY
            };
        }

        public new void Dispose()
        {
            if (!disposedValue)
            {
                if (imageModel?.ImageProperties is ImagePropertiesModel props)
                    props.PropertyChanged -= ImageModelPropertyChange;

                imageModel = null;
                image = null;
                disposedValue = true;
            }
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
