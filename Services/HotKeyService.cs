using Avalonia;
using Avalonia.Input;
using Avalonia.Win32.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Services
{
    public class HotKeyService
    {
        private IntPtr _windowHandle;
        public event Action? HotKeyPressed;

        public HotKeyService()
        {
            //RegisterHotKeys();
        }

        //private void RegisterHotKeys()
        //{
        //    // Subscribe to window messages
        //    var wndProc = new WndProc((hWnd, msg, wParam, lParam) =>
        //    {
        //        if (msg == WM_HOTKEY)
        //        {
        //            // Handle hotkey press here
        //            int hotKeyId = wParam.ToInt32();
        //            OnHotKeyPressed(hotKeyId);
        //            return IntPtr.Zero;
        //        }
        //        return DefWindowProc(hWnd, msg, wParam, lParam);
        //    }
        //}

        private void UnregisterHotKey()
        {
        }

        public void OnHotKeyPressed()
        {
            HotKeyPressed?.Invoke();
        }
    }
}
