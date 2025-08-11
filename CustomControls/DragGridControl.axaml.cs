using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace UltrawideOverlays.CustomControls
{

    /// <summary>
    /// A Panel made for Overlay Editor, to allow dragging and dropping controls around the screen with the Mouse. It'll only allow dragging controls to direct children, so it shouldn't disturb subpanels.
    /// </summary>
    /// <remarks>
    /// WARNING! This class will rebase its children as children of the DragPanelLayout. Be careful if that matters for some reason.
    /// </remarks>
    public partial class DragGridControl : Panel
    {
        ///////////////////////////////////////////
        /// STYLED PROPERTIES
        ///////////////////////////////////////////

        /// <summary>
        /// Grid-size Styled Property
        /// </summary>
        public static readonly StyledProperty<int> GridSizeProperty =
            AvaloniaProperty.Register<DragGridControl, int>(nameof(GridSize));

        public int GridSize
        {
            get => this.GetValue(GridSizeProperty);
            set => SetValue(GridSizeProperty, value);
        }

        public static readonly StyledProperty<Boolean> PreviewProperty =
            AvaloniaProperty.Register<DragGridControl, Boolean>(nameof(Preview));

        public bool Preview
        {
            get => this.GetValue(PreviewProperty);
            set => SetValue(PreviewProperty, value);
        }

        public static readonly StyledProperty<Color?> ColorProperty =
            AvaloniaProperty.Register<DragGridControl, Color?>(nameof(Color));

        public Color? Color
        {
            get => this.GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly StyledProperty<double> GridOpacityProperty =
            AvaloniaProperty.Register<DragGridControl, double>(nameof(GridOpacity));

        public double GridOpacity
        {
            get => this.GetValue(GridOpacityProperty);
            set => SetValue(GridOpacityProperty, value);
        }

        private DragPanel? _dragPanel;
        private VisualGrid? _visualGrid;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        public DragGridControl()
        {
            InitializeComponent();

            // Namescope is created by InitializeComponent, so resolve once here
            _dragPanel = this.FindControl<DragPanel>("DragPanelLayout");
            _visualGrid = this.FindControl<VisualGrid>("VisualGrid");
        }

        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == GridSizeProperty)
            {
                UpdateGridSize((int)change.NewValue!); // Update the grid size of the DragPanelLayout
            }
            else if (change.Property == GridOpacityProperty)
            {
                if (_visualGrid != null)
                {
                    _visualGrid.Opacity = (double)change.NewValue!;
                }
            }
            else if (change.Property == ColorProperty)
            {
                // Update the grid brush of the DragPanelLayout
                if (_visualGrid != null && change.NewValue != null)
                {
                    _visualGrid.Color = (Color)change.NewValue;
                }
            }
            else if (change.Property == PreviewProperty)
            {
                // Update the grid preview of the DragPanelLayout
                if (_visualGrid != null)
                {
                    _visualGrid.Preview = (bool)change.NewValue!;
                }
            }
            base.OnPropertyChanged(change);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Clone the list first to avoid modifying it while iterating
            var toMove = this.Children
                .Where(c => c is Control control && control is not DragPanel)
                .ToList();

            foreach (var control in toMove)
            {
                this.Children.Remove(control);
                _dragPanel?.Children.Add(control);
            }

            toMove.Clear();
        }

        /// <summary>
        /// Rebase the children as children of DragPanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            base.ChildrenChanged(sender, e);

            if (e.NewItems == null || _dragPanel == null)
                return;

            // Clone the items
            var newControls = e.NewItems
                .OfType<Control>()
                .Where(c => c is not DragPanel)
                .ToList();

            foreach (var control in newControls)
            {
                this.Children.Remove(control);
                _dragPanel.Children.Add(control);
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);

            _dragPanel = null;
            _visualGrid = null;

            // Reset the properties
            Color = null;
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

        private void UpdateGridSize(int value)
        {
            // Update the grid size of the DragPanelLayout
            if (_dragPanel != null)
            {
                _dragPanel.SnappingGridSize = value;
            }
            if (_visualGrid != null)
            {
                _visualGrid.GridSize = value;
            }
        }
    }
}