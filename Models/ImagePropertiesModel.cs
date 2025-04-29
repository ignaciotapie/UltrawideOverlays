using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace UltrawideOverlays.Models
{
    public partial class ImagePropertiesModel : ObservableObject
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

        [ObservableProperty]
        private int _width;

        [ObservableProperty]
        private int _height;

        [ObservableProperty]
        private double _scale = 1;

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
    }
}
