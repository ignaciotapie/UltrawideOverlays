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
        AvaloniaProperty.Register<VisualGrid, int?>("GridSize");

    public int? GridSize
    {
        get => GetValue(GridSizeProperty);
        set => SetValue(GridSizeProperty, value);
    }

    public static readonly StyledProperty<IBrush?> ColorProperty =
        AvaloniaProperty.Register<VisualGrid, IBrush?>("Color");

    public IBrush? Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly StyledProperty<Boolean> PreviewProperty =
        AvaloniaProperty.Register<VisualGrid, Boolean>(nameof(Preview), true);

    public bool Preview
    {
        get => GetValue(PreviewProperty);
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
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == GridSizeProperty)
        {
            InvalidateVisual();
        }
        else if (change.Property == ColorProperty)
        {
            InvalidateVisual();
        }
        else if (change.Property == PreviewProperty)
        {
            InvalidateVisual();
        }
    }

    protected override Size MeasureOverride(Size finalSize)
    {
        return finalSize;
    }

    public sealed override void Render(DrawingContext context)
    {
        if (GridSize != null && Preview)
        {
            //Else this causes an infinite loop
            if (GridSize.Value <= 0)
            {
                return;
            }
            var pen = new Pen(Color, 1);
            for (int i = 0; i < DesiredSize.Width; i += GridSize.Value)
            {
                context.DrawLine(pen, new Point(i, 0), new Point(i, DesiredSize.Height));
            }
            for (int i = 0; i < DesiredSize.Height; i += GridSize.Value)
            {
                context.DrawLine(pen, new Point(0, i), new Point(DesiredSize.Width, i));
            }
        }

        base.Render(context);
    }
}