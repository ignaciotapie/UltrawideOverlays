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

        public SettingsPageViewModel()
        {
            Page = Enums.ApplicationPageViews.SettingsPage;
            PageName = SettingsPageName;
        }

        ~SettingsPageViewModel()
        {
            Debug.WriteLine("SettingsPageViewModel finalized!");
        }

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
            SettingsData = origSettings.Clone() as SettingsDataModel; //Return a clone so we don't save over the original in memory
        }

        public async Task SaveSettingsAsync()
        {
            if (SettingsData != null)
            {
                await SettingsDataService.SaveSettingsAsync(SettingsData);
            }
            else
            {
                Debug.WriteLine("SettingsData is null, cannot save settings.");
            }
        }

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
        }

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

        //TODO: add hotkey rebinding
        //[RelayCommand]
        //private async void EditHotkey(object obj) 
        //{
        //    if (obj is string hotkey) 
        //    {
        //        switch (hotkey)
        //        {
        //            case "OpacityUp":
        //                break;
        //            case "OpacityDown":
        //                break;
        //            case "Toggle":
        //            default:
        //                break;
        //        }
        //    }
        //}
    }
}
