using CommunityToolkit.Mvvm.ComponentModel;
using System;
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

        private OverlayDataService OverlayDataService;
        private FocusMonitorService FocusMonitorService;
        private GamesDataService GamesDataService;
        private SettingsDataService SettingsDataService;
        private HotKeyService HotKeyService;
        private ActivityDataService ActivityDataService;

        private ImageWrapperDecorator imageWrapperDecorator;

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
                    Debug.WriteLine(SelectedOverlay.ToString());
                    _ = ActivityDataService.SaveActivity(ActivityLogType.Overlays, ActivityLogAction.Viewed, overlay.Name);
                }
                else
                {
                    if (SelectedOverlay != null)
                    {
                        SelectedOverlay.Dispose();
                        Debug.WriteLine(SelectedOverlay.ToString());
                    }
                    SelectedOverlay = null; // No overlay found for this game
                }
            }
            else
            {
                if (SelectedOverlay != null) 
                { 
                    SelectedOverlay.Dispose(); 
                    Debug.WriteLine(SelectedOverlay.ToString());
                }
                SelectedOverlay = null; // No game found for this file path
            }
        }

        private void DecreaseOverlayOpacity()
        {
            if (SelectedOverlay != null && ImageOpacity.HasValue && ImageOpacity >= 0.1)
            {
                if (!IsOverlayEnabled) { IsOverlayEnabled = true; return; }
                ImageOpacity -= 0.1;
            }
        }

        private void IncreaseOverlayOpacity()
        {
            if (SelectedOverlay != null && ImageOpacity.HasValue && ImageOpacity <= 0.9)
            {
                if (!IsOverlayEnabled) { IsOverlayEnabled = true; return; }
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
            OverlayDataService = null!;
            FocusMonitorService = null!;
            GamesDataService = null!;
            SettingsDataService = null!;
            HotKeyService = null!;
            ActivityDataService = null!;
            imageWrapperDecorator = null!;

            _selectedOverlay = null;
            _imageOpacity = null;

            GC.SuppressFinalize(this);
        }
    }
}
