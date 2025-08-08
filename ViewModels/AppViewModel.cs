using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
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
        private bool _trayEnabled;

        private readonly SettingsDataService SettingsService;
        private readonly WindowFactory WindowFactory;
        private readonly HotKeyService HotKeyService;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        public AppViewModel(SettingsDataService settingsService, HotKeyService hotKeyService, WindowFactory windowFactory)
        {
            SettingsService = settingsService;
            SettingsService.SettingsChanged += SettingsChangedHandler;
            WindowFactory = windowFactory;
            HotKeyService = hotKeyService;

            _ = SetUpTrayIcon();
            _ = SetUpStartUp();

            HotKeyService.HotKeyPressed += HotKeyPressed;
        }

        ~AppViewModel()
        {
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

        private void HotKeyPressed(object? sender, string e)
        {
            if (e == SettingsNames.OpenMiniOverlayManager)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var miniOverlayManagerWindow = WindowFactory.CreateWindow(WindowViews.MiniOverlayManager);
                    miniOverlayManagerWindow.Show();
                });
            }
        }

        private void SettingsChangedHandler(object? sender, SettingsChangedArgs e)
        {
            if (e.SettingsChanged.Any(s => s.Name == SettingsNames.MinimizeToTray))
            {
                _ = SetUpTrayIcon();
            }
            if (e.SettingsChanged.Any(s => s.Name == SettingsNames.StartupEnabled))
            {
                _ = SetUpStartUp();
            }
        }

        private async Task SetUpTrayIcon()
        {
            var setting = await SettingsService.LoadSettingAsync(SettingsNames.MinimizeToTray);
            TrayEnabled = setting == SettingsBoolValues.True;
        }

        private async Task SetUpStartUp()
        {
            var setting = await SettingsService.LoadSettingAsync(SettingsNames.StartupEnabled);
            bool enabled = setting == SettingsBoolValues.True;

            if (enabled && !AutoStartUtil.IsAutostartEnabled())
            {
                AutoStartUtil.EnableAutostart();
            }
            else if (!enabled && AutoStartUtil.IsAutostartEnabled())
            {
                AutoStartUtil.DisableAutostart();
            }
        }

        //Shouldn't be ever called lmao
        public override void Dispose()
        {
            SettingsService.SettingsChanged -= SettingsChangedHandler;
            HotKeyService.HotKeyPressed -= HotKeyPressed;

            Debug.WriteLine("AppViewModel finalized!");
        }
    }
}
