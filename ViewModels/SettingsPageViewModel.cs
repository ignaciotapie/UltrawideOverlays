using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Threading.Tasks;
using UltrawideOverlays.Enums;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;

namespace UltrawideOverlays.ViewModels
{
    public partial class SettingsPageViewModel : PageViewModel
    {

        private SettingsDataModel _settingsData;
        private SettingsDataModel SettingsData { get => _settingsData; set => UpdatePageSettings(value); }
        private SettingsDataService SettingsDataService { get; set; }

        [ObservableProperty]
        private string _settingsPageName = "Settings";

        //Settings
        [ObservableProperty]
        private SingleSettingModel _gridSize;
        [ObservableProperty]
        private SingleSettingModel _gridOpacity;
        [ObservableProperty]
        private SingleSettingModel _gridColor;
        [ObservableProperty]
        private SingleSettingModel _startUpEnabled;
        [ObservableProperty]
        private SingleSettingModel _minimizeToTray;
        [ObservableProperty]
        private SingleSettingModel _toggleOverlayHotkey;
        [ObservableProperty]
        private SingleSettingModel _opacityUpHotkey;
        [ObservableProperty]
        private SingleSettingModel _opacityDownHotkey;
        [ObservableProperty]
        private SingleSettingModel _quickOverlayHotkey;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        public SettingsPageViewModel()
        {
            Page = Enums.ApplicationPageViews.SettingsPage;
            PageName = SettingsPageName;
        }

        ~SettingsPageViewModel()
        {
            Dispose();
        }

        ///////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        ///////////////////////////////////////////

        public SettingsPageViewModel(SettingsDataService settingsService)
        {
            Page = Enums.ApplicationPageViews.SettingsPage;
            PageName = SettingsPageName;

            SettingsDataService = settingsService;

            LoadSettingsAsync();
        }

        public async Task LoadSettingsAsync()
        {
            var origSettings = await SettingsDataService.LoadSettingsAsync();
            SettingsData = origSettings;
        }

        public async Task SaveSettingsAsync()
        {
            if (SettingsData != null)
            {
                //Return clone so there's no memory sharing between new settings and saved
                await SettingsDataService.SaveSettingsAsync(SettingsData.Clone());
            }
            else
            {
                Debug.WriteLine("SettingsData is null, cannot save settings.");
            }
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

        private void UpdatePageSettings(SettingsDataModel value)
        {
            if (value == null)
            {
                Debug.WriteLine("SettingsData is null, cannot update page settings.");
                return;
            }

            _settingsData = value;

            GridSize = value.SettingsDictionary[SettingsNames.GridSize];
            GridOpacity = value.SettingsDictionary[SettingsNames.GridOpacity];
            GridColor = value.SettingsDictionary[SettingsNames.GridColor];
            StartUpEnabled = value.SettingsDictionary[SettingsNames.StartupEnabled];
            MinimizeToTray = value.SettingsDictionary[SettingsNames.MinimizeToTray];
            ToggleOverlayHotkey = value.SettingsDictionary[SettingsNames.ToggleOverlayHotkey];
            OpacityUpHotkey = value.SettingsDictionary[SettingsNames.OpacityUpHotkey];
            OpacityDownHotkey = value.SettingsDictionary[SettingsNames.OpacityDownHotkey];
            QuickOverlayHotkey = value.SettingsDictionary[SettingsNames.OpenMiniOverlayManager];
        }


        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////


        [RelayCommand]
        private async Task SaveSettings()
        {
            await SaveSettingsAsync();
        }

        [RelayCommand]
        private void GetDefaultSettings()
        {
            SettingsData = SettingsDataService.LoadDefaultSettings();
        }

        public override void Dispose()
        {
            Debug.WriteLine("SettingsPageViewModel finalized!");
        }
    }
}
