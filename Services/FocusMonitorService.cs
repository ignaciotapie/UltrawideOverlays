using Interop.UIAutomationClient;
using System;
using System.Diagnostics;

namespace UltrawideOverlays.Services
{
    public class FocusChangedHandler : IUIAutomationFocusChangedEventHandler
    {
        public event Action<string> FocusChanged;

        private string lastExePath;

        public void HandleFocusChangedEvent(IUIAutomationElement sender)
        {
            try
            {
                int processId = sender.CurrentProcessId;
                var process = Process.GetProcessById(processId);
                string? exePath = process.MainModule?.FileName;
                if (lastExePath != exePath)
                {
                    FocusChanged.Invoke(exePath);
                    lastExePath = exePath;
                }

                Debug.WriteLine($"Focus changed to: {sender.CurrentName}");
                Debug.WriteLine($"Process: {process.ProcessName}, PID: {processId}, Path: {exePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error handling focus change: {ex.Message}");
            }
        }
    }

    public class FocusMonitorService : IDisposable
    {
        private CUIAutomation _automation;
        private FocusChangedHandler _handler;

        public event Action<string> FocusChanged;

        public FocusMonitorService()
        {
            _automation = new CUIAutomation();
            _handler = new FocusChangedHandler();
            _handler.FocusChanged += (exePath) => FocusChanged?.Invoke(exePath);
            _automation.AddFocusChangedEventHandler(null, _handler);
        }

        public void Dispose()
        {
            try
            {
                _automation.RemoveFocusChangedEventHandler(_handler);
            }
            catch { }
        }
    }
}
