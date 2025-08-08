using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using UltrawideOverlays.Decorator;
using UltrawideOverlays.Enums;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
using UltrawideOverlays.Wrappers;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayViewModel : ViewModelBase
    {
        [ObservableProperty]
        private OverlayWrapper? _selectedOverlay;

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

        private readonly ImageWrapperDecorator imageWrapperDecorator;

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
                                ActivityDataService activityService,
                                ImageWrapperDecorator wrapperDecorator)
        {
            OverlayDataService = overlayDataService;
            FocusMonitorService = focusMonitorService;
            GamesDataService = gamesDataService;
            SettingsDataService = settingsDataService;
            HotKeyService = hotKeyService;
            ActivityDataService = activityService;
            imageWrapperDecorator = wrapperDecorator;
            HotKeyService.RegisterHotKeys();

            ImageOpacity = 1;
            IsOverlayEnabled = true;

            FocusMonitorService.FocusChanged += FocusChangedHandler;
            HotKeyService.HotKeyPressed += HotkeyPressedHandler;
        }

        ~OverlayViewModel()
        {
            Dispose();
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
                    SelectedOverlay = imageWrapperDecorator.CreateOverlayWrapper(overlay, overlay.Path);
                    ActivityDataService.SaveActivity(new ActivityLogModel(System.DateTime.Now, ActivityLogType.Overlays, ActivityLogAction.Viewed, overlay.Name));
                }
                else
                {
                    if (SelectedOverlay != null) SelectedOverlay.Dispose();
                    SelectedOverlay = null; // No overlay found for this game
                }
            }
            else
            {
                if (SelectedOverlay != null) SelectedOverlay.Dispose();
                SelectedOverlay = null; // No game found for this file path
            }
        }

        private void DecreaseOverlayOpacity()
        {
            if (SelectedOverlay != null && ImageOpacity.HasValue && ImageOpacity >= 0.1)
            {
                ImageOpacity -= 0.1;
            }
        }

        private void IncreaseOverlayOpacity()
        {
            if (SelectedOverlay != null && ImageOpacity.HasValue && ImageOpacity <= 0.9)
            {
                ImageOpacity += 0.1;
            }
        }

        private void ToggleOverlay()
        {
            if (SelectedOverlay != null)
            {
                IsOverlayEnabled = !IsOverlayEnabled;
            }
        }

        public override void Dispose()
        {
            Debug.WriteLine("OverlayViewModel finalized!");
            HotKeyService.UnregisterHotKeys();

            if (SelectedOverlay != null)
            {
                SelectedOverlay.Dispose();
                SelectedOverlay = null;
            }

            FocusMonitorService.FocusChanged -= FocusChangedHandler;
            HotKeyService.HotKeyPressed -= HotkeyPressedHandler;
        }
    }
}
