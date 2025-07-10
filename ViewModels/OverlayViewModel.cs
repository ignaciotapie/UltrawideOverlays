using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlayViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string? imageSource = null;

        private readonly OverlayDataService OverlayDataService;
        private readonly FocusMonitorService FocusMonitorService;
        private readonly GamesDataService GamesDataService;

        private ICollection<OverlayDataModel> Overlays;
        private ICollection<GamesModel> Games;

        /// <summary>
        /// Design-time constructor
        /// </summary>
        public OverlayViewModel()
        {
        }

        public OverlayViewModel(OverlayDataService overlayDataService, GamesDataService gamesDataService, FocusMonitorService focusMonitorService)
        {
            OverlayDataService = overlayDataService;
            FocusMonitorService = focusMonitorService;
            GamesDataService = gamesDataService;

            FocusMonitorService.FocusChanged += FocusChanged;
        }
        ~OverlayViewModel()
        {
            Debug.WriteLine("OverlayViewModel finalized!");
        }

        private async void FocusChanged(string filePath)
        {
            // Check if the focused game is in the list of games
            var game = await GamesDataService.LoadGameAsync(filePath);
            if (game != null)
            {
                // Get the overlay for the focused game
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
