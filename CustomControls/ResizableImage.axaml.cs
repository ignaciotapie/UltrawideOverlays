using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace UltrawideOverlays.CustomControls
{
    public partial class ResizableImage : UserControl
    {
        private Point _dragOffset;
        private bool _isDragging;

        ///////////////////////////////////////////
        /// STYLED PROPERTIES
        ///////////////////////////////////////////
        public static readonly StyledProperty<Bitmap?> ImageSourceProperty =
            AvaloniaProperty.Register<ResizableImage, Bitmap?>("ImageSource", defaultValue: null, validate: (value) => value is Bitmap || value is null);

        public Bitmap? ImageSource
        {
            get => GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        public ResizableImage()
        {
            InitializeComponent();
            this.PointerPressed += (_, __) => Console.WriteLine("ResizableImage pressed");
            //this.PointerPressed += OnPointerPressed;
            //this.PointerReleased += OnPointerReleased;
            //this.PointerMoved += OnPointerMoved;
        }

        ///////////////////////////////////////////
        /// OVERIDDEN FUNCTIONS
        ///////////////////////////////////////////
        public override void Render(DrawingContext context)
        {
            if (this.IsFocused)
            {
                context.FillRectangle(Brushes.Blue, new Rect(Bounds.Size));
            }

            base.Render(context);
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            _isDragging = true;
            e.Pointer.Capture(this);
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            _isDragging = false;
            e.Pointer.Capture(null);
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!_isDragging) return;
        }
    }
}
