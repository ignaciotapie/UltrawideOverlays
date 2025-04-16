using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ExCSS;
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
        /// <summary>
        /// Grid-size Styled Property
        /// </summary>
        public static readonly StyledProperty<int> GridSizeProperty =
            AvaloniaProperty.Register<DragGridControl, int>(nameof(GridSize), 50);

        public int GridSize
        {
            get => this.GetValue(GridSizeProperty);
            set => SetValue(GridSizeProperty, value);
        }

        public static readonly StyledProperty<Boolean> PreviewProperty =
            AvaloniaProperty.Register<DragGridControl, Boolean>(nameof(Preview), true);

        public bool Preview
        {
            get => VisualGrid.Preview;
            set => VisualGrid.Preview = value;
        }

        public static readonly StyledProperty<IBrush?> BrushProperty =
            AvaloniaProperty.Register<DragGridControl, IBrush?>("Brush", defaultValue: Brushes.Gray, validate: (brush) => brush is not null);

        public IBrush? Brush
        {
            get => VisualGrid.Brush;
            set => VisualGrid.Brush = value;
        }

        public static readonly StyledProperty<double> GridOpacityProperty =
            AvaloniaProperty.Register<DragGridControl, double>(nameof(GridOpacity), 0.5);

        public double GridOpacity
        {
            get => VisualGrid.Opacity;
            set => VisualGrid.Opacity = value;
        }

        public DragGridControl()
        {
            InitializeComponent();
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


        //Rebase the children as children of DragPanel
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
    }
}