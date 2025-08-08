using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace UltrawideOverlays.Models
{
    public partial class ImagePropertiesModel : ModelBase
    {
        private Point _position;
        public Point Position
        {
            get => _position;
            set
            {
                if (SetProperty(ref _position, value))
                {
                    _positionX = value.X;
                    _positionY = value.Y;
                    OnPropertyChanged(nameof(PositionX));
                    OnPropertyChanged(nameof(PositionY));
                }
            }
        }

        private double _positionX;
        public double PositionX
        {
            get => _positionX;
            set
            {
                if (SetProperty(ref _positionX, value))
                {
                    _position = new Point(_positionX, _positionY);
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        private double _positionY;
        public double PositionY
        {
            get => _positionY;
            set
            {
                if (SetProperty(ref _positionY, value))
                {
                    _position = new Point(_positionX, _positionY);
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        private double _originalWidth;
        private double _originalHeight;
        public double OriginalWidth
        {
            get => _originalWidth;
            set => _originalWidth = value;
        }
        public double OriginalHeight
        {
            get => _originalHeight;
            set => _originalHeight = value;
        }

        [ObservableProperty]
        private double _width;

        [ObservableProperty]
        private double _height;

        private double _scale = 1;

        public double Scale
        {
            get => _scale;
            set
            {
                if (SetProperty(ref _scale, value))
                {
                    Width = Math.Max(1, OriginalWidth * value);
                    Height = Math.Max(1, OriginalHeight * value);
                    _scale = value;
                }
            }
        }

        [ObservableProperty]
        private bool _isHMirrored = false;

        [ObservableProperty]
        private bool _isVMirrored = false;

        [ObservableProperty]
        private double _opacity = 1;

        [ObservableProperty]
        private bool _isVisible = true;

        [ObservableProperty]
        private bool _isDraggable = true;

        [ObservableProperty]
        private int _zIndex = 0;

        public override string ToString()
        {
            return $"ImagePropertiesModel: Position=({PositionX}, {PositionY}), Size=({Width}x{Height}), Scale={Scale}, " +
                   $"Mirroring=({IsHMirrored}, {IsVMirrored}), Opacity={Opacity}, Visible={IsVisible}, Draggable={IsDraggable}, ZIndex={ZIndex}";
        }

        public override ImagePropertiesModel Clone()
        {
            return new ImagePropertiesModel
            {
                Position = this.Position,
                OriginalWidth = this.OriginalWidth,
                OriginalHeight = this.OriginalHeight,
                Width = this.Width,
                Height = this.Height,
                Scale = this.Scale,
                IsHMirrored = this.IsHMirrored,
                IsVMirrored = this.IsVMirrored,
                Opacity = this.Opacity,
                IsVisible = this.IsVisible,
                IsDraggable = this.IsDraggable,
                ZIndex = this.ZIndex
            };
        }
    }
}
