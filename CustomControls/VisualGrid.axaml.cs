using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;

namespace UltrawideOverlays.CustomControls;

public partial class VisualGrid : UserControl
{
    ///////////////////////////////////////////
    /// STYLED PROPERTIES
    ///////////////////////////////////////////
    public static readonly StyledProperty<int?> GridSizeProperty =
        AvaloniaProperty.Register<VisualGrid, int?>("GridSize", defaultValue: 5, validate: (value) => value is int);

    public int? GridSize
    {
        get => GetValue(GridSizeProperty);
        set => SetValue(GridSizeProperty, value);
    }

    public static readonly StyledProperty<IBrush?> BrushProperty =
        AvaloniaProperty.Register<VisualGrid, IBrush?>("Brush", defaultValue: Brushes.Gray, validate: (brush) => brush is not null);

    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public static readonly StyledProperty<Boolean> PreviewProperty =
        AvaloniaProperty.Register<VisualGrid, Boolean>(nameof(Preview), true);

    public bool Preview
    {
        get => this.GetValue(PreviewProperty);
        set => SetValue(PreviewProperty, value);
    }

    ///////////////////////////////////////////
    /// CONSTRUCTOR
    ///////////////////////////////////////////
    public VisualGrid()
    {
        InitializeComponent();
    }

    ///////////////////////////////////////////
    /// OVERRIDE FUNCTIONS
    ///////////////////////////////////////////
    protected override Size MeasureOverride(Size finalSize)
    {
        return finalSize;
    }

    public sealed override void Render(DrawingContext context)
    {
        if (GridSize != null && Preview)
        {
            var renderSize = Bounds.Size;
            var pen = new Pen(Brush, 1);
            for (int i = 0; i < renderSize.Width; i += GridSize.Value)
            {
                context.DrawLine(pen, new Point(i, 0), new Point(i, renderSize.Height));
            }
            for (int i = 0; i < renderSize.Height; i += GridSize.Value)
            {
                context.DrawLine(pen, new Point(0, i), new Point(renderSize.Width, i));
            }
        }

        base.Render(context);
    }
}