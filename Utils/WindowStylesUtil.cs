using Avalonia.Controls;
using System;

namespace UltrawideOverlays.Utils
{
    public static class WindowExStyles
    {
        public static readonly Int32 Transparent = 0x00000020;
    }

    public static class WindowStylesUtil
    {
        public static void AddStyleToWindow(Window window, uint style, uint exStyle)
        {
            // Add custom window styles
            Win32Properties.AddWindowStylesCallback(window, (s, e) => (s | style, e | exStyle));
        }
    }
}
