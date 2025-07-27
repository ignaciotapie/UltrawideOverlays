using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UltrawideOverlays.Enums;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Services;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.ViewModels
{
    public partial class AppViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _iconPath;

        [ObservableProperty]
        private bool _trayEnabled;

        private readonly SettingsDataService SettingsService;
        private readonly WindowFactory WindowFactory;
        private readonly HotKeyService HotKeyService;

        public AppViewModel(SettingsDataService settingsService, HotKeyService hotKeyService, WindowFactory windowFactory)
        {
            SettingsService = settingsService;
            SettingsService.SettingsChanged += SettingsChangedHandler;
            WindowFactory = windowFactory;
            HotKeyService = hotKeyService;

            SetUpTrayIcon();
            SetUpStartUp();

            HotKeyService.HotKeyPressed += HotKeyPressed;
        }

        private void HotKeyPressed(object? sender, string e)
        {
            switch (e)
            {
                case SettingsNames.OpenMiniOverlayManager:
                    Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                    {
                        var miniOverlayManagerWindow = WindowFactory.CreateWindow(WindowViews.MiniOverlayManager);
                        miniOverlayManagerWindow.Show();
                    });
                    break;
            }
        }

        private void SettingsChangedHandler(object? sender, SettingsChangedArgs e)
        {
            if (e.SettingsChanged.Any(s => s.Name == SettingsNames.MinimizeToTray))
            {
                SetUpTrayIcon();
            }
            if (e.SettingsChanged.Any(s => s.Name == SettingsNames.StartupEnabled))
            {
                SetUpStartUp();
            }
        }

        private async Task SetUpTrayIcon()
        {
            var setting = await SettingsService.LoadSettingAsync(SettingsNames.MinimizeToTray);

            if (setting == SettingsBoolValues.True)
            {
                TrayEnabled = true;
            }
            else
            {
                TrayEnabled = false;
            }
        }

        private async Task SetUpStartUp()
        {
            var setting = await SettingsService.LoadSettingAsync(SettingsNames.StartupEnabled);

            if (setting == SettingsBoolValues.True)
            {
                if (!AutoStartUtil.IsAutostartEnabled())
                {
                    AutoStartUtil.EnableAutostart();
                }
            }
            else if (AutoStartUtil.IsAutostartEnabled())
            {
                AutoStartUtil.DisableAutostart();
            }
        }
    }
}
