using CommunityToolkit.Mvvm.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UltrawideOverlays.Enums;
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

        public AppViewModel(SettingsDataService settingsService)
        {
            SettingsService = settingsService;
            SettingsService.SettingsChanged += SettingsChangedHandler;

            SetUpTrayIcon();
            SetUpStartUp();
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
