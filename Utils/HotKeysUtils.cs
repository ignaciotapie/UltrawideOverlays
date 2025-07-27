using System;
using System.Runtime.InteropServices;

namespace UltrawideOverlays.Utils
{
    public static class HotKeysUtils
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        // Message structure
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        public struct MODIFIER_KEYS
        {
            public const uint ALT = 0x0001;
            public const uint CONTROL = 0x0002;
            public const uint SHIFT = 0x0004;
            public const uint WIN = 0x0008;
            public const uint NOREPEAT = 0x4000;
        }

        public struct KEYS
        {
            public const uint O = 0x4F; // Virtual key code for 'O'
            public const uint UP = 0x26; // Virtual key code for Up Arrow
            public const uint DOWN = 0x28; // Virtual key code for Down Arrow
            public const uint P = 0x50; // Virtual key code for 'P'
        }

        public struct HOTKEY_UNIQUEID
        {
            public const int ToggleOverlay = 9001; // Unique ID for Toggle Overlay hotkey
            public const int OpacityUp = 9002; // Unique ID for Opacity Up hotkey
            public const int OpacityDown = 9003; // Unique ID for Opacity Down hotkey
            public const int OpenMiniOverlayManager = 9004; // Unique ID for Open Mini Overlay Manager hotkey
        }

        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
    }
}
