using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.ComponentModel;
using System.Diagnostics;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Models;

namespace UltrawideOverlays.CustomControls
{
    public class SelectableImage : Border, IDisposable
    {
        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<SelectableImage, bool>(nameof(IsSelected));

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public ImageModel? imageModel;
        public Image? image;
        private bool disposedValue;

        public event EventHandler<ImageModel>? ImageSelected;

        public SelectableImage(ImageModel im)
        {
            imageModel = im;
            DataContext = im;
            Background = Brushes.Transparent;
            BorderBrush = null;
            BorderThickness = new Thickness(2);
            image = new Image
            {
                IsHitTestVisible = false
            };
            image.Bind(Image.SourceProperty, new Binding("ImagePath") { Converter = PathToBitmapConverter.Converter });
            image.Bind(Image.WidthProperty, new Binding("ImageProperties.Width", BindingMode.TwoWay));
            image.Bind(Image.HeightProperty, new Binding("ImageProperties.Height", BindingMode.TwoWay));
            image.Bind(Image.OpacityProperty, new Binding("ImageProperties.Opacity", BindingMode.TwoWay));
            image.Bind(Image.IsVisibleProperty, new Binding("ImageProperties.IsVisible", BindingMode.TwoWay));

            Bind(WidthProperty, new Binding("ImageProperties.Width", BindingMode.TwoWay));
            Bind(HeightProperty, new Binding("ImageProperties.Height", BindingMode.TwoWay));
            Bind(IsHitTestVisibleProperty, new Binding("ImageProperties.IsDraggable", BindingMode.TwoWay));
            Bind(DragPanel.PositionProperty, new Binding("ImageProperties.Position", BindingMode.TwoWay));

            Child = image;
        }

        static SelectableImage()
        {
            IsSelectedProperty.Changed.AddClassHandler<SelectableImage>((img, e) =>
            {
                img.UpdateBorder();
            });
        }

        ~SelectableImage()
        {
            Debug.WriteLine("SelectableImage finalized");
        }


        ///////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        ///////////////////////////////////////////
        public void SelectImage()
        {
            if (imageModel == null)
                return;
            ImageSelected?.Invoke(this, imageModel);
        }

        ///////////////////////////////////////////
        /// HANDLERS
        ///////////////////////////////////////////
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (imageModel != null)
            {
                SelectImage();
            }
        }
        private void ImageModelPropertyChange(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ImagePropertiesModel.IsHMirrored) ||
                e.PropertyName == nameof(ImagePropertiesModel.IsVMirrored) ||
                e.PropertyName == nameof(ImagePropertiesModel.Scale))
            {
                UpdateTransform();
            };
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

            PointerPressed += OnPointerPressed;
            base.OnAttachedToVisualTree(e);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            // Listen for mirror or scale changes
            if (imageModel?.ImageProperties is ImagePropertiesModel props)
            {
                props.PropertyChanged -= ImageModelPropertyChange;
            }

            PointerPressed -= OnPointerPressed;
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (imageModel?.ImageProperties is ImagePropertiesModel props)
                    props.PropertyChanged -= ImageModelPropertyChange;

                PointerPressed -= OnPointerPressed;
                ImageSelected = null;
                imageModel = null;
                image = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
