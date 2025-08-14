using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
using UltrawideOverlays.Utils;

namespace UltrawideOverlays.ViewModels
{
    public partial class QuickOverlayWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ICollection<OverlayDataModel> _overlays;

        [ObservableProperty]
        private OverlayDataModel _selectedOverlay;

        [ObservableProperty]
        private string _appName;

        [ObservableProperty]
        private string _appPath;

        private GamesDataService GamesService;
        private OverlayDataService OverlayService;
        private FocusMonitorService FocusMonitorService;
        private WindowFactory WindowFactory;
        private Window? windowInstance;

        private bool isDisposed = false;

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

        /// <summary>
        /// Design-only constructor
        /// </summary>
        public QuickOverlayWindowViewModel()
        {

        }

        public QuickOverlayWindowViewModel(OverlayDataService overlayService, GamesDataService gamesDataService, FocusMonitorService focusMonitorService, WindowFactory factory)
        {
            OverlayService = overlayService;
            FocusMonitorService = focusMonitorService;
            GamesService = gamesDataService;
            WindowFactory = factory;

            LoadData();
            FindApp();
        }

        ~QuickOverlayWindowViewModel()
        {
            Dispose();
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

        private async void LoadData()
        {
            Overlays = await OverlayService.LoadAllOverlaysAsync();
        }

        private async void FindApp()
        {
            AppName = FileHandlerUtil.GetFileName(FocusMonitorService.LastFocusedExePath);
            AppPath = FocusMonitorService.LastFocusedExePath;

            if (await GamesService.LoadGameAsync(FocusMonitorService.LastFocusedExePath) is GamesModel game)
            {
                AppName = game.Name;
                AppPath = game.ExecutablePath;
                SelectedOverlay = Overlays.FirstOrDefault(o => o.Name.Equals(game.OverlayName, StringComparison.OrdinalIgnoreCase));
            }
        }

        private void OnOverlayEditorWindowClosed(object? sender, EventArgs e)
        {
            windowInstance.Closed -= OnOverlayEditorWindowClosed;
            windowInstance = null;
            LoadData();
        }

        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////

        [RelayCommand]
        private async void AddGame(object parameter)
        {
            if (string.IsNullOrEmpty(AppPath) || string.IsNullOrEmpty(AppName))
            {
                return;
            }

            var game = new GamesModel
            {
                Name = AppName,
                OverlayName = SelectedOverlay?.Name ?? string.Empty,
                ExecutablePath = AppPath
            };

            await GamesService.SaveGameAsync(game);

            if (parameter is Window window)
            {
                window.Close();
            }
        }

        [RelayCommand]
        private void Reset()
        {
            SelectedOverlay = null;
        }

        [RelayCommand]
        private void OpenNewOverlayWindow()
        {
            if (windowInstance != null && windowInstance.IsVisible)
            {
                return;
            }
            //TODO: This is violating MVVM, VM shouldn't interfere with views, we can ask the App to open it but not to influence it...
            windowInstance = WindowFactory.CreateWindow(Enums.WindowViews.OverlayEditorWindow);
            windowInstance.Show();
            windowInstance.Closed += OnOverlayEditorWindowClosed;
        }

        public override void Dispose()
        {
            if (isDisposed) return;

            isDisposed = true;
            if (windowInstance != null)
            {
                windowInstance.Closed -= OnOverlayEditorWindowClosed;
                windowInstance.DataContext = null;
                windowInstance = null;
            }

            FocusMonitorService = null;
            OverlayService = null;
            GamesService = null;
            WindowFactory = null;
        }
    }
}
