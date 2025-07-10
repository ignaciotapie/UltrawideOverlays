using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace UltrawideOverlays.CustomControls
{
    public class DragPanel : Panel
    {
        private Control? _draggedControl;
        private SelectableItemBase? _selectableControl;
        private Point _dragOffset;
        private Point _initialPosition;

        ///////////////////////////////////////////
        /// STYLED PROPERTIES
        ///////////////////////////////////////////
        public static readonly AttachedProperty<Point> PositionProperty =
            AvaloniaProperty.RegisterAttached<DragPanel, Control, Point>("Position", new Point(0, 0));

        public static readonly AttachedProperty<Boolean> SnapToGridProperty =
        AvaloniaProperty.RegisterAttached<DragPanel, Control, Boolean>("SnapToGrid", true);

        public static void SetSnapToGrid(Control control, bool value) => control.SetValue(SnapToGridProperty, value);
        public static bool GetSnapToGrid(Control control) => control.GetValue(SnapToGridProperty);

        public static Point GetPosition(Control control) => control.GetValue(PositionProperty);
        public static void SetPosition(Control control, Point value)
        {
            Debug.WriteLine($"Setting position of {control} to {value}");
            if (GetDraggable(control))
            {
                control.SetValue(PositionProperty, value);
            }
        }

        public static readonly StyledProperty<int> SnappingGridSizeProperty =
        AvaloniaProperty.Register<DragPanel, int>("SnappingGridSize");
        public int? SnappingGridSize
        {
            get => GetValue(SnappingGridSizeProperty);
            set => SetValue(SnappingGridSizeProperty, value);
        }

        public static readonly StyledProperty<bool> DraggableProperty =
            AvaloniaProperty.RegisterAttached<DragPanel, Control, bool>("Draggable", true);
        public static void SetDraggable(Control control, bool value) => control.SetValue(DraggableProperty, value);
        public static bool GetDraggable(Control control) => control.GetValue(DraggableProperty);

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        public DragPanel()
        {
            PointerPressed += OnPointerPressed;
            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;
            Children.CollectionChanged += OnChildrenChanged;
        }

        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            PointerPressed -= OnPointerPressed;
            PointerMoved -= OnPointerMoved;
            PointerReleased -= OnPointerReleased;
            Children.CollectionChanged -= OnChildrenChanged;

            foreach (var newItem in Children)
            {
                if (newItem is Control control)
                {
                    control.PropertyChanged -= OnChildPropertyChanged;
                }
            }

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

        private void OnChildrenChanged(object? arg1, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in args.NewItems!)
                {
                    if (newItem is Control control)
                    {
                        control.PropertyChanged += OnChildPropertyChanged;
                    }
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Remove && args.OldItems != null)
            {
                foreach (var oldItem in args.OldItems)
                {
                    if (oldItem is Control control)
                    {
                        control.PropertyChanged -= OnChildPropertyChanged;
                    }
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Replace)
            {
                if (args.OldItems != null)
                {
                    foreach (var oldItem in args.OldItems)
                    {
                        if (oldItem is Control control)
                        {
                            control.PropertyChanged -= OnChildPropertyChanged;
                        }
                    }
                }
                if (args.NewItems != null)
                {
                    foreach (var newItem in args.NewItems)
                    {
                        if (newItem is Control control)
                        {
                            control.PropertyChanged += OnChildPropertyChanged;
                        }
                    }
                }
            }
            else if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                // Unsubscribe from all previously subscribed children
                foreach (var child in Children)
                {
                    if (child is Control control)
                    {
                        control.PropertyChanged -= OnChildPropertyChanged;
                    }
                }

                // Then re-subscribe to current ones
                foreach (var child in Children)
                {
                    if (child is Control control)
                    {
                        control.PropertyChanged += OnChildPropertyChanged;
                    }
                }
            }
        }

        private void OnChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == PositionProperty)
            {
                InvalidateArrange();
            }
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
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

                if (control is SelectableItemBase selectableItem && _selectableControl != selectableItem)
                {
                    if (_selectableControl != null) { _selectableControl.IsSelected = false; }
                    _selectableControl = selectableItem;
                    selectableItem.IsSelected = true;
                }

                e.Pointer.Capture(this);
            }
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (_draggedControl != null)
            {
                var pointerPos = e.GetPosition(this);
                var newPos = pointerPos - _dragOffset;

                if (GetPosition(_draggedControl) == newPos)
                {
                    // No movement, no need to update position
                    return;
                }

                SetPosition(_draggedControl, newPos);
            }
        }

        private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (_draggedControl != null)
            {
                if (!_initialPosition.NearlyEquals(GetPosition(_draggedControl)))
                {
                    SnapToGrid(_draggedControl);
                }

                _draggedControl = null;
            }
            e.Pointer.Capture(null);
        }
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
