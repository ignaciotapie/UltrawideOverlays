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

        public static readonly StyledProperty<IBrush?> ColorProperty =
            AvaloniaProperty.Register<DragGridControl, IBrush?>(nameof(Color));

        public IBrush? Color
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

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        public DragGridControl()
        {
            InitializeComponent();
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
                // Update the grid opacity of the DragPanelLayout
                if (DragPanelLayout != null)
                {
                    VisualGrid.Opacity = (double)change.NewValue!;
                }
            }
            else if (change.Property == ColorProperty)
            {
                // Update the grid brush of the DragPanelLayout
                if (VisualGrid != null)
                {
                    VisualGrid.Color = (IBrush)change.NewValue!;
                }
            }
            else if (change.Property == PreviewProperty)
            {
                // Update the grid preview of the DragPanelLayout
                if (VisualGrid != null)
                {
                    VisualGrid.Preview = (bool)change.NewValue!;
                }
            }
            base.OnPropertyChanged(change);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var child in Children)
            {
                if (child is Control control)
                {
                    var pos = DragPanel.GetPosition(control);
                    control.Arrange(new Rect(pos, control.DesiredSize));
                }
            }
            return finalSize;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            // Clone the list first to avoid modifying it while iterating
            var toMove = this.Children
                .Where(c => c is Control control && control is not DragPanel)
                .ToList(); // this is the key

            foreach (var control in toMove)
            {
                this.Children.Remove(control);              // remove from self
                DragPanelLayout.Children.Add(control);      // add to target layout
            }
        }

        /// <summary>
        /// Rebase the children as children of DragPanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            base.ChildrenChanged(sender, e);

            if (e.NewItems == null || DragPanelLayout == null)
                return;

            // Clone the items
            var newControls = e.NewItems
                .OfType<Control>()
                .Where(c => c is not DragPanel)
                .ToList();

            foreach (var control in newControls)
            {
                this.Children.Remove(control);
                DragPanelLayout.Children.Add(control);
            }
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

        private void UpdateGridSize(int value)
        {
            // Update the grid size of the DragPanelLayout
            if (DragPanelLayout != null)
            {
                DragPanelLayout.SnappingGridSize = value;
            }
            if (VisualGrid != null)
            {
                VisualGrid.GridSize = value;
            }
        }
    }
}