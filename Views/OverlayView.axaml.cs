using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using System;
using System.Diagnostics;

namespace UltrawideOverlays.Views;

public partial class OverlayView : Window
{
    public static readonly StyledProperty<string> ImageSourceExistsProperty =
        AvaloniaProperty.Register<OverlayView, string>(nameof(ImageSourceExists));

    public string ImageSourceExists
    {
        get => this.GetValue(ImageSourceExistsProperty);
        set => SetValue(ImageSourceExistsProperty, value);
    }

    public OverlayView()
    {
        InitializeComponent();

        // Set window properties
        this.Position = new PixelPoint(0, 0);
        this.Width = Screens.Primary.Bounds.Width; //TODO: Settings monitor selection?
        this.Height = Screens.Primary.Bounds.Height;
        this.CanResize = false;

        Bind(ImageSourceExistsProperty, new Binding("ImageSource")
        {
            Converter = Avalonia.Data.Converters.StringConverters.IsNotNullOrEmpty
        });
    }

    ///////////////////////////////////////////
    /// OVERRIDE FUNCTIONS
    ///////////////////////////////////////////
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        ApplyWindowStyles();
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        // Prevent the window from being closed
        e.Cancel = true;
        this.Hide();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ImageSourceExistsProperty)
        {
            if (change.NewValue is string exists && exists == "True")
            {
                // If the image source exists, show the window
                this.Show();
                Debug.WriteLine("OverlayView Window SHOWN");
            }
            else
            {
                // If the image source does not exist, hide the window
                this.Hide();
                Debug.WriteLine("OverlayView Window HIDDEN!!");
            }
        }

        base.OnPropertyChanged(change);
    }

    ///////////////////////////////////////////
    /// PRIVATE FUNCTIONS
    ///////////////////////////////////////////
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