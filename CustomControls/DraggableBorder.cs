using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UltrawideOverlays.CustomControls
{
    /// <summary>
    /// Mostly a test of how custom controls work, it's a border that can be dragged around the screen.
    /// </summary>
    public class DraggableBorder : Border
    {
        private bool _isPressed;
        private Point _positionInBlock;
        private TranslateTransform _transform = null!;

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            _isPressed = true;
            _positionInBlock = e.GetPosition((Visual?)Parent);

            if (_transform != null!)
                _positionInBlock = new Point(
                    _positionInBlock.X - _transform.X,
                    _positionInBlock.Y - _transform.Y);

            base.OnPointerPressed(e);
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            _isPressed = false;

            base.OnPointerReleased(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            if (!_isPressed)
                return;

            if (Parent == null)
                return;

            var currentPosition = e.GetPosition((Visual?)Parent);

            var offsetX = currentPosition.X - _positionInBlock.X;
            var offsetY = currentPosition.Y - _positionInBlock.Y;

            _transform = new TranslateTransform(offsetX, offsetY);
            RenderTransform = _transform;

            base.OnPointerMoved(e);
        }
    }
}
