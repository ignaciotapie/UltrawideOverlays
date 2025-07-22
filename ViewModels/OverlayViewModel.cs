using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? _imageSource;
        [ObservableProperty]
        private double? _imageOpacity;

        private readonly OverlayDataService OverlayDataService;
        private readonly FocusMonitorService FocusMonitorService;
        private readonly GamesDataService GamesDataService;
        private readonly SettingsDataService SettingsDataService;
        private readonly HotKeyService HotKeyService;

        private ICollection<OverlayDataModel> Overlays;
        private ICollection<GamesModel> Games;

        /// <summary>
        /// Design-time constructor
        /// </summary>
        public OverlayViewModel()
        {
        }

        public OverlayViewModel(OverlayDataService overlayDataService, GamesDataService gamesDataService, SettingsDataService settingsDataService, FocusMonitorService focusMonitorService)
        {
            OverlayDataService = overlayDataService;
            FocusMonitorService = focusMonitorService;
            GamesDataService = gamesDataService;
            SettingsDataService = settingsDataService;
            //HotKeyService = hotKeyService;

            ImageOpacity = 1;

            SettingsDataService.SettingsChanged += SettingsChangedHandler;
            FocusMonitorService.FocusChanged += FocusChanged;
        }

        private void SettingsChangedHandler(object? sender, SettingsChangedArgs e)
        {

        }

        ~OverlayViewModel()
        {
            Debug.WriteLine("OverlayViewModel finalized!");
        }

        private async void FocusChanged(string filePath)
        {
            var game = await GamesDataService.LoadGameAsync(filePath);
            if (game != null)
            {
                var overlay = await OverlayDataService.LoadOverlayAsync(game.OverlayName);
                if (overlay != null)
                {
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
    }
}
