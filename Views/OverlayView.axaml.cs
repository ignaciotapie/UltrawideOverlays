using Avalonia.Controls;
using Avalonia.Interactivity;

namespace UltrawideOverlays.Views;

public partial class OverlayView : Window
{
    public OverlayView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        // Add custom window styles
        Win32Properties.AddWindowStylesCallback(this, addWindowStyle);
    }

    private (uint style, uint exStyle) addWindowStyle(uint style, uint exStyle)
    {
        // Add WS_EX_LAYERED to the extended style
        return (style, exStyle | 0x00000020);
    }
}