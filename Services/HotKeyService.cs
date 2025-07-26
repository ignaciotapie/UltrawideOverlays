using System;
using System.Diagnostics;
using System.Threading;
using UltrawideOverlays.Enums;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Services
{
    public class HotKeyService
    {
        public event EventHandler<string>? HotKeyPressed;

        private Thread HotkeyThread;

        public HotKeyService()
        {
            RegisterHotKeys();
        }

        private void RegisterHotKeys()
        {
            if (HotkeyThread == null)
            {
                HotkeyThread = new Thread(() =>
                {
                    if (!HotKeysUtils.RegisterHotKey(IntPtr.Zero, HotKeysUtils.HotKeyId, HotKeysUtils.MODIFIER_KEYS.CONTROL | HotKeysUtils.MODIFIER_KEYS.ALT, HotKeysUtils.KEYS.O)) //Ctrl + Alt + O
                    {
                        throw new InvalidOperationException("Failed to register hotkey for Ctrl + Alt + O.");
                    }

                    if (!HotKeysUtils.RegisterHotKey(IntPtr.Zero, HotKeysUtils.HotKeyId + 1, HotKeysUtils.MODIFIER_KEYS.CONTROL | HotKeysUtils.MODIFIER_KEYS.ALT, HotKeysUtils.KEYS.UP)) //Ctrl + Alt + Up
                    {
                        throw new InvalidOperationException("Failed to register hotkey for Ctrl + Alt + Up.");
                    }

                    if (!HotKeysUtils.RegisterHotKey(IntPtr.Zero, HotKeysUtils.HotKeyId + 2, HotKeysUtils.MODIFIER_KEYS.CONTROL | HotKeysUtils.MODIFIER_KEYS.ALT, HotKeysUtils.KEYS.DOWN)) //Ctrl + Alt + Down
                    {
                        throw new InvalidOperationException("Failed to register hotkey for Ctrl + Alt + Down.");
                    }

                    if (!HotKeysUtils.RegisterHotKey(IntPtr.Zero, HotKeysUtils.HotKeyId + 3, HotKeysUtils.MODIFIER_KEYS.CONTROL | HotKeysUtils.MODIFIER_KEYS.ALT, HotKeysUtils.KEYS.P)) //Ctrl + Alt + P
                    {
                        throw new InvalidOperationException("Failed to register hotkey for Ctrl + Alt + P.");
                    }

                    // Listen for hotkey events
                    while (true)
                    {
                        HotKeysUtils.MSG msg;
                        if (HotKeysUtils.GetMessage(out msg, IntPtr.Zero, 0, 0))
                        {
                            if (msg.message == HotKeysUtils.WM_HOTKEY)
                            {
                                switch (msg.wParam.ToInt32())
                                {
                                    case HotKeysUtils.HotKeyId:
                                        Debug.WriteLine("Ctrl + Alt + O pressed");
                                        OnHotKeyPressed(SettingsNames.ToggleOverlayHotkey);
                                        break;
                                    case HotKeysUtils.HotKeyId + 1:
                                        Debug.WriteLine("Ctrl + Alt + Up pressed");
                                        OnHotKeyPressed(SettingsNames.OpacityUpHotkey);
                                        break;
                                    case HotKeysUtils.HotKeyId + 2:
                                        Debug.WriteLine("Ctrl + Alt + Down pressed");
                                        OnHotKeyPressed(SettingsNames.OpacityDownHotkey);
                                        break;
                                    case HotKeysUtils.HotKeyId + 3:
                                        Debug.WriteLine("Ctrl + Alt + P pressed");
                                        OnHotKeyPressed(SettingsNames.OpenMiniOverlayManager);
                                        break;
                                }
                            }
                        }
                    }
                });
                HotkeyThread.IsBackground = true;

                HotkeyThread.Start();
            }
        }

        private void UnregisterHotKey()
        {
            HotkeyThread.Join();
            if (!HotKeysUtils.UnregisterHotKey(IntPtr.Zero, HotKeysUtils.HotKeyId))
            {
                throw new InvalidOperationException("Failed to unregister hotkey.");
            }
            if (!HotKeysUtils.UnregisterHotKey(IntPtr.Zero, HotKeysUtils.HotKeyId + 1))
            {
                throw new InvalidOperationException("Failed to unregister hotkey for Ctrl + Alt + Up.");
            }
            if (!HotKeysUtils.UnregisterHotKey(IntPtr.Zero, HotKeysUtils.HotKeyId + 2))
            {
                throw new InvalidOperationException("Failed to unregister hotkey for Ctrl + Alt + Down.");
            }
            if (!HotKeysUtils.UnregisterHotKey(IntPtr.Zero, HotKeysUtils.HotKeyId + 3))
            {
                throw new InvalidOperationException("Failed to unregister hotkey for Ctrl + Alt + P.");
            }
        }

        public void OnHotKeyPressed(string Code)
        {
            Debug.WriteLine($"HotKey Pressed!! Code:{Code}");
            HotKeyPressed?.Invoke(this, Code);
        }
    }
}
