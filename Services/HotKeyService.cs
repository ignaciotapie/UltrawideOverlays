using System;
using System.Diagnostics;
using System.Threading;
using UltrawideOverlays.Enums;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.Services
{
    public class HotKeyService : IDisposable
    {
        public event EventHandler<string>? HotKeyPressed;

        private Thread? _hotkeyThread;
        private CancellationTokenSource? _cancelToken;
        private bool _disposed;

        private readonly nint NULL_WINDOW_HANDLE = IntPtr.Zero;

        public void Dispose()
        {
            if (_disposed)
                return;

            UnregisterHotKeys();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        ~HotKeyService()
        {
            Dispose();
        }

        public void RegisterHotKeys()
        {
            if (_hotkeyThread != null)
                return;

            _cancelToken = new CancellationTokenSource();

            _hotkeyThread = new Thread(() =>
            {
                try
                {
                    RegisterAllHotKeys();

                    while (!_cancelToken!.IsCancellationRequested)
                    {
                        if (HotKeysUtils.GetMessage(out var msg, NULL_WINDOW_HANDLE, 0, 0))
                        {
                            if (msg.message == HotKeysUtils.WM_HOTKEY)
                            {
                                HandleHotkeyPressed(msg.wParam.ToInt32());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Hotkey thread error: {ex.Message}");
                }
            });

            _hotkeyThread.IsBackground = true;
            _hotkeyThread.Start();
        }

        public void UnregisterHotKeys()
        {
            _cancelToken?.Cancel();
            _hotkeyThread?.Join();

            HotKeysUtils.UnregisterHotKey(NULL_WINDOW_HANDLE, HotKeysUtils.HOTKEY_UNIQUEID.ToggleOverlay);
            HotKeysUtils.UnregisterHotKey(NULL_WINDOW_HANDLE, HotKeysUtils.HOTKEY_UNIQUEID.OpacityUp);
            HotKeysUtils.UnregisterHotKey(NULL_WINDOW_HANDLE, HotKeysUtils.HOTKEY_UNIQUEID.OpacityDown);
            HotKeysUtils.UnregisterHotKey(NULL_WINDOW_HANDLE, HotKeysUtils.HOTKEY_UNIQUEID.OpenMiniOverlayManager);

            _hotkeyThread = null;
            _cancelToken?.Dispose();
            _cancelToken = null;
        }

        private void RegisterAllHotKeys()
        {
            RegisterHotkey(HotKeysUtils.HOTKEY_UNIQUEID.ToggleOverlay, HotKeysUtils.KEYS.O, SettingsNames.ToggleOverlayHotkey);
            RegisterHotkey(HotKeysUtils.HOTKEY_UNIQUEID.OpacityUp, HotKeysUtils.KEYS.UP, SettingsNames.OpacityUpHotkey);
            RegisterHotkey(HotKeysUtils.HOTKEY_UNIQUEID.OpacityDown, HotKeysUtils.KEYS.DOWN, SettingsNames.OpacityDownHotkey);
            RegisterHotkey(HotKeysUtils.HOTKEY_UNIQUEID.OpenMiniOverlayManager, HotKeysUtils.KEYS.P, SettingsNames.OpenMiniOverlayManager);
        }

        private void RegisterHotkey(int id, uint key, string name)
        {
            bool success = HotKeysUtils.RegisterHotKey(NULL_WINDOW_HANDLE, id,
                HotKeysUtils.MODIFIER_KEYS.CONTROL | HotKeysUtils.MODIFIER_KEYS.ALT, key);

            if (!success)
                throw new InvalidOperationException($"Failed to register hotkey for {name}.");
        }

        private void HandleHotkeyPressed(int id)
        {
            String? hotkey;

            switch (id)
            {
                case HotKeysUtils.HOTKEY_UNIQUEID.ToggleOverlay:
                    hotkey = SettingsNames.ToggleOverlayHotkey;
                    break;
                case HotKeysUtils.HOTKEY_UNIQUEID.OpacityUp:
                    hotkey = SettingsNames.OpacityUpHotkey;
                    break;
                case HotKeysUtils.HOTKEY_UNIQUEID.OpacityDown:
                    hotkey = SettingsNames.OpacityDownHotkey;
                    break;
                case HotKeysUtils.HOTKEY_UNIQUEID.OpenMiniOverlayManager:
                    hotkey = SettingsNames.OpenMiniOverlayManager;
                    break;
                default:
                    hotkey = null;
                    break;
            };

            if (hotkey != null)
            {
                Debug.WriteLine($"Hotkey triggered: {hotkey}");
                HotKeyPressed?.Invoke(this, hotkey);
            }
        }
    }
}
