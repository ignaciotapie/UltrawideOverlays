using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using UltrawideOverlays.Enums;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? _imageSource;

        [ObservableProperty]
        private double? _imageOpacity;

        [ObservableProperty]
        private bool _isOverlayEnabled;

        private readonly OverlayDataService OverlayDataService;
        private readonly FocusMonitorService FocusMonitorService;
        private readonly GamesDataService GamesDataService;
        private readonly SettingsDataService SettingsDataService;
        private readonly HotKeyService HotKeyService;
        private readonly ActivityDataService ActivityDataService;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        /// <summary>
        /// Design-time constructor
        /// </summary>
        public OverlayViewModel()
        {
        }

        public OverlayViewModel(OverlayDataService overlayDataService,
                                GamesDataService gamesDataService,
                                SettingsDataService settingsDataService,
                                FocusMonitorService focusMonitorService,
                                HotKeyService hotKeyService,
                                ActivityDataService activityService)
        {
            OverlayDataService = overlayDataService;
            FocusMonitorService = focusMonitorService;
            GamesDataService = gamesDataService;
            SettingsDataService = settingsDataService;
            HotKeyService = hotKeyService;
            ActivityDataService = activityService;

            ImageOpacity = 1;
            IsOverlayEnabled = true;

            FocusMonitorService.FocusChanged += FocusChangedHandler;
            HotKeyService.HotKeyPressed += HotkeyPressedHandler;
        }

        ~OverlayViewModel()
        {
            Debug.WriteLine("OverlayViewModel finalized!");

            FocusMonitorService.FocusChanged -= FocusChangedHandler;
            HotKeyService.HotKeyPressed -= HotkeyPressedHandler;
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

        private void HotkeyPressedHandler(object? sender, string e)
        {
            switch (e)
            {
                case SettingsNames.ToggleOverlayHotkey:
                    ToggleOverlay();
                    break;
                case SettingsNames.OpacityUpHotkey:
                    IncreaseOverlayOpacity();
                    break;
                case SettingsNames.OpacityDownHotkey:
                    DecreaseOverlayOpacity();
                    break;
                default:
                    Debug.WriteLine($"Unknown hotkey action: {e}");
                    break;
            }
        }

        private async void FocusChangedHandler(string filePath)
        {
            var game = await GamesDataService.LoadGameAsync(filePath);
            if (game != null)
            {
                var overlay = await OverlayDataService.LoadOverlayAsync(game.OverlayName);
                if (overlay != null)
                {
                    ActivityDataService.SaveActivity(new ActivityLogModel(System.DateTime.Now, ActivityLogType.Overlays, ActivityLogAction.Viewed, overlay.Name));
                    ImageSource = overlay.Path;
                }
                else
                {
                    ImageSource = null; // No overlay found for this game
                }
            }
            else
            {
                ImageSource = null; // No game found for this file path
            }
        }

        private void DecreaseOverlayOpacity()
        {
            if (ImageSource != null && ImageOpacity.HasValue && ImageOpacity >= 0.1)
            {
                ImageOpacity -= 0.1;
            }
        }

        private void IncreaseOverlayOpacity()
        {
            if (ImageSource != null && ImageOpacity.HasValue && ImageOpacity <= 0.9)
            {
                ImageOpacity += 0.1;
            }
        }

        private void ToggleOverlay()
        {
            if (ImageSource != null)
            {
                IsOverlayEnabled = !IsOverlayEnabled;
            }
        }
    }
}
