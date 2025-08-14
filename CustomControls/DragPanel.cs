using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

namespace UltrawideOverlays.CustomControls
{
    public class DragPanel : Panel
    {
        ///////////////////////////////////////////
        /// STYLED PROPERTIES
        ///////////////////////////////////////////

        public static readonly AttachedProperty<Boolean> SnapToGridProperty =
        AvaloniaProperty.RegisterAttached<DragPanel, Control, Boolean>("SnapToGrid", true);
        public static void SetSnapToGrid(Control control, bool value) => control.SetValue(SnapToGridProperty, value);
        public static bool GetSnapToGrid(Control control) => control.GetValue(SnapToGridProperty);

        public static readonly AttachedProperty<Point> PositionProperty =
            AvaloniaProperty.RegisterAttached<DragPanel, Control, Point>("Position", new Point(0, 0));
        public static Point GetPosition(Control control) => control.GetValue(PositionProperty);
        public static void SetPosition(Control control, Point value)
        {
            Debug.WriteLine($"Setting position of {control} to {value}");
            if (GetDraggable(control))
            {
                control.SetValue(PositionProperty, value);
            }
        }

        public static readonly AttachedProperty<bool> DraggableProperty =
            AvaloniaProperty.RegisterAttached<DragPanel, Control, bool>("Draggable", true);
        public static void SetDraggable(Control control, bool value) => control.SetValue(DraggableProperty, value);
        public static bool GetDraggable(Control control) => control.GetValue(DraggableProperty);

        public static readonly StyledProperty<int> SnappingGridSizeProperty =
        AvaloniaProperty.Register<DragPanel, int>(nameof(SnappingGridSize));
        public int? SnappingGridSize
        {
            get => GetValue(SnappingGridSizeProperty);
            set => SetValue(SnappingGridSizeProperty, value);
        }

        private Control? _draggedControl;
        private Point _dragOffset;
        private Point _initialPosition;
        private IPointer _capturedPointer;

        private Dictionary<Control, IDisposable> _posSubscriptions;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        public DragPanel()
        {
            _posSubscriptions = new Dictionary<Control, IDisposable>();
        }

        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            var control = e.Source as Control;
            Debug.WriteLine($"Pointer pressed on {control}");
            if (control != null)
            {
                _draggedControl = control;
                var pointerPos = e.GetPosition(this);
                var controlPos = GetPosition(control);
                _dragOffset = pointerPos - controlPos;
                _initialPosition = controlPos;
                _draggedControl.Focus();

                e.Pointer.Capture(this);
                _capturedPointer = e.Pointer;
            }
            else
            {
                _draggedControl = null;
            }
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (_draggedControl != null)
            {
                var pointerPos = e.GetPosition(this);
                var newPos = pointerPos - _dragOffset;

                if (GetPosition(_draggedControl) != newPos)
                {
                    SetPosition(_draggedControl, newPos);
                }
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            if (_draggedControl != null)
            {
                if (!_initialPosition.NearlyEquals(GetPosition(_draggedControl)))
                {
                    SnapToGrid(_draggedControl);
                }

                _draggedControl = null;
            }
            e.Pointer.Capture(null);
            _capturedPointer = null;
        }
        protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
        {
            _capturedPointer?.Capture(null);
            _capturedPointer = null;
            _draggedControl = null;
            base.OnPointerCaptureLost(e);
        }
        protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            base.ChildrenChanged(sender, e);

            if (e.Action is NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var c in e.NewItems.OfType<Control>())
                    AttachToChild(c);
            }
            else if (e.Action is NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach (var c in e.OldItems.OfType<Control>())
                    DetachFromChild(c);
            }
            else if (e.Action is NotifyCollectionChangedAction.Replace)
            {
                if (e.OldItems != null)
                    foreach (var c in e.OldItems.OfType<Control>()) DetachFromChild(c);
                if (e.NewItems != null)
                    foreach (var c in e.NewItems.OfType<Control>()) AttachToChild(c);
            }
            else if (e.Action is NotifyCollectionChangedAction.Reset)
            {
                foreach (var kv in _posSubscriptions.Values) kv.Dispose();
                _posSubscriptions.Clear();
            }
        }

        private void AttachToChild(Control c)
        {
            DetachFromChild(c);

            var sub = c.GetObservable(PositionProperty)
                       .Subscribe(_ => InvalidateArrange());
            _posSubscriptions.Add(c, sub);
        }

        private void DetachFromChild(Control c)
        {
            if (_posSubscriptions.Remove(c, out var sub))
                sub.Dispose();
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            foreach (var c in Children.OfType<Control>())
                AttachToChild(c);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            _capturedPointer?.Capture(null);
            _capturedPointer = null;

            foreach (var sub in _posSubscriptions.Values) sub.Dispose();
            _posSubscriptions.Clear();

            _draggedControl = null;

            base.OnDetachedFromVisualTree(e);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var pos = GetPosition(child);
                var size = child.DesiredSize;

                child.Arrange(new Rect(pos, size));
            }
            return finalSize;
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

        private void SnapToGrid(Control control)
        {
            if (SnappingGridSize != null && GetSnapToGrid(control))
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
