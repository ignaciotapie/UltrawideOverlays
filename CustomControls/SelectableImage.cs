using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Models;
using UltrawideOverlays.CustomControls;
using Svg;
using Avalonia.Data.Converters;

namespace UltrawideOverlays.CustomControls
{
    public class SelectableImage : Border
    {
        public static readonly StyledProperty<bool> IsSelectedProperty =
            AvaloniaProperty.Register<SelectableImage, bool>(nameof(IsSelected));

        public bool IsSelected
        {
            get => GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public ImageModel? imageModel;
        public Image image;

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
                [!Image.SourceProperty] = new Binding("ImagePath")
                {
                    Converter = PathToBitmapConverter.Converter
                },
                [!Image.WidthProperty] = new Binding("ImageProperties.Width", BindingMode.TwoWay),
                [!Image.HeightProperty] = new Binding("ImageProperties.Height", BindingMode.TwoWay),
                [!Image.OpacityProperty] = new Binding("ImageProperties.Opacity", BindingMode.TwoWay),
                [!Image.IsVisibleProperty] = new Binding("ImageProperties.IsVisible", BindingMode.TwoWay),
                IsHitTestVisible = false
            };

            Bind(WidthProperty, new Binding("ImageProperties.Width", BindingMode.TwoWay));
            Bind(HeightProperty, new Binding("ImageProperties.Height", BindingMode.TwoWay));
            Bind(IsHitTestVisibleProperty, new Binding("ImageProperties.IsDraggable", BindingMode.TwoWay));
            Bind(DragPanel.PositionProperty, new Binding("ImageProperties.Position", BindingMode.TwoWay));

            Child = image;

            PointerPressed += OnPointerPressed;

            // Listen for mirror or scale changes
            if (imageModel?.ImageProperties is ImagePropertiesModel props)
            {
                props.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(ImagePropertiesModel.IsHMirrored) ||
                        e.PropertyName == nameof(ImagePropertiesModel.IsVMirrored) ||
                        e.PropertyName == nameof(ImagePropertiesModel.Scale))
                    {
                        UpdateTransform();
                    }
                };

                UpdateTransform(); // Initial update
            }
        }

        static SelectableImage()
        {
            IsSelectedProperty.Changed.AddClassHandler<SelectableImage>((img, e) =>
            {
                img.UpdateBorder();
            });
        }


        ///////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        ///////////////////////////////////////////
        public void SelectImage() 
        {
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


        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////
        protected override Size MeasureOverride(Size availableSize)
        {
            return availableSize;
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
    }
}
