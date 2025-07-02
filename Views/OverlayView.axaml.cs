using Avalonia;
using Avalonia.Controls;
using System;

namespace UltrawideOverlays.Views;

public partial class OverlayView : Window
{
    public OverlayView()
    {
        InitializeComponent();

        // Set window properties
        this.Position = new PixelPoint(0, 0);
        this.Width = Screens.Primary.Bounds.Width; //TODO: Settings monitor selection?
        this.Height = Screens.Primary.Bounds.Height;
        this.CanResize = false;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        ApplyWindowStyles();
    }

    private void ApplyWindowStyles()
    {
        var platformHandle = this.TryGetPlatformHandle();
        if (platformHandle?.Handle != null)
        {
            var hwnd = platformHandle.Handle;

            // Get current extended style
            var exStyle = (uint)NativeWindowUtils.GetWindowLong(hwnd, NativeWindowUtils.GWL_EXSTYLE);

            // Set new styles
            exStyle |= NativeWindowUtils.WS_EX_LAYERED
                    | NativeWindowUtils.WS_EX_TRANSPARENT
                    | NativeWindowUtils.WS_EX_NOACTIVATE
                    | NativeWindowUtils.WS_EX_TOOLWINDOW;

            NativeWindowUtils.SetWindowLong(
                hwnd,
                NativeWindowUtils.GWL_EXSTYLE,
                (IntPtr)exStyle);

            // Optional: Set layered window attributes
            NativeWindowUtils.SetLayeredWindowAttributes(hwnd, 0, 255, 0x2 /* LWA_ALPHA */);
        }
    }
}