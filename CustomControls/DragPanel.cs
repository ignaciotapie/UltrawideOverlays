using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Diagnostics;

namespace UltrawideOverlays.CustomControls
{
    public class DragPanel : Panel
    {
        private Control? _draggedControl;
        private Point _dragOffset;

        ///////////////////////////////////////////
        /// STYLED PROPERTIES
        ///////////////////////////////////////////
        private static readonly AttachedProperty<Point> PositionProperty =
            AvaloniaProperty.RegisterAttached<DragPanel, Control, Point>("Position", new Point(0, 0));

        public static Point GetPosition(Control control) => control.GetValue(PositionProperty);
        public static void SetPosition(Control control, Point value)
        {
            Debug.WriteLine($"Setting position of {control} to {value}");
            control.SetValue(PositionProperty, value);
        }

        public static readonly StyledProperty<int> SnappingGridSizeProperty =
        AvaloniaProperty.Register<DragPanel, int>("SnappingGridSize");
        public int? SnappingGridSize
        {
            get => GetValue(SnappingGridSizeProperty);
            set => SetValue(SnappingGridSizeProperty, value);
        }

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        public DragPanel()
        {
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;
        }

        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var pos = GetPosition(child);
                child.Arrange(new Rect(pos, child.DesiredSize));
            }
            return finalSize;
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////
        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var control = e.Source as Control;
            if (control != null)
            {
                _draggedControl = control;
                var pointerPos = e.GetPosition(this);
                var controlPos = GetPosition(control);
                _dragOffset = pointerPos - controlPos;
                _draggedControl.Focus();

                e.Pointer.Capture(this);
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_draggedControl != null)
            {
                var pointerPos = e.GetPosition(this);
                var newPos = pointerPos - _dragOffset;
                SetPosition(_draggedControl, newPos);
                InvalidateArrange();
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_draggedControl != null)
            {
                //Snap to grid
                SnapToGrid(_draggedControl);

                _draggedControl = null;
                e.Pointer.Capture(null);
            }
        }
        private void SnapToGrid(Control control)
        {
            if (SnappingGridSize != null)
            {
                var gridSize = SnappingGridSize.Value;
                var pos = GetPosition(control);
                var positionSnapped = new Point(Math.Round(pos.X / gridSize) * gridSize, Math.Round(pos.Y / gridSize) * gridSize);
                SetPosition(control, positionSnapped);
            }

            InvalidateArrange();
        }
    }
}
