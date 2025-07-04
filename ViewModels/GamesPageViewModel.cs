using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
using UltrawideOverlays.Utils;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace UltrawideOverlays.ViewModels
{
    public partial class GamesPageViewModel : PageViewModel
    {
        [ObservableProperty]
        private ObservableCollection<ProcessDataModel> _processes;

        [ObservableProperty]
        private ObservableCollection<GamesModel> _games;

        [ObservableProperty]
        private ObservableCollection<OverlayDataModel> _overlays;

        [ObservableProperty]
        private string _gameExecutablePath;

        [ObservableProperty]
        private ProcessDataModel? _selectedProcess;

        [ObservableProperty]
        private OverlayDataModel? _selectedOverlay;

        [ObservableProperty]
        private String _gameName;

        [ObservableProperty]
        private GamesModel? _selectedGame;

        private PageFactory _factory;
        private ProcessDataService _processService;
        private OverlayDataService _overlayService;
        private GamesDataService _gamesService;

        /// <summary>
        /// Design-only constructor
        /// </summary>
        public GamesPageViewModel()
        {
            Page = Enums.ApplicationPageViews.GamesPage;
            PageName = "Games";
            Bitmap icon;
            using (var memoryStream = new MemoryStream())
            {
                SystemIcons.Application.ToBitmap()?.Save(memoryStream, ImageFormat.Png);
                memoryStream.Position = 0;
                icon = new Bitmap(memoryStream);
            }

            Processes =
            [
                new ProcessDataModel()
                {
                    Name = "Game.exe",
                    Path = "C:\\Games\\Game.exe",
                    Icon = icon,
                    ProcessId = 1234
                },
                new ProcessDataModel() {
                    Name = "Game.exe",
                    Path = "C:\\Games\\Game.exe",
                    Icon = icon,
                    ProcessId = 1234
                },
                new ProcessDataModel() {
                    Name = "Game.exe",
                    Path = "C:\\Games\\Game.exe",
                    Icon = icon,
                    ProcessId = 1234
                }
            ];

            Games =
            [
                new GamesModel()
                {
                    Name = "Game 1",
                    OverlayName = "Game 1 Overlay",
                    ExecutablePath = "C:\\Images\\Game1.exe"
                },
                new GamesModel()
                {
                    Name = "Game 2",
                    OverlayName = "Game 2 Overlay",
                    ExecutablePath = "C:\\Images\\Game2.exe"
                },
                new GamesModel()
                {
                    Name = "Game 3",
                    OverlayName = "Game 3 Overlay",
                    ExecutablePath = "C:\\Images\\Game3.exe"
                },
            ];

            Overlays = new ObservableCollection<OverlayDataModel>()
            {
                new OverlayDataModel() { Name = "Overlay 1"},
                new OverlayDataModel() { Name = "Overlay 2"},
                new OverlayDataModel() { Name = "Overlay 3"}
            };
        }


        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////
        public GamesPageViewModel(PageFactory PFactory, ProcessDataService processService, OverlayDataService overlayService, GamesDataService gamesService)
        {
            Page = Enums.ApplicationPageViews.GamesPage;
            PageName = "Games";

            _factory = PFactory;
            _processService = processService;
            _overlayService = overlayService;
            _gamesService = gamesService;

            LoadProcessesAsync();
            LoadOverlaysAsync();
            LoadGamesAsync();
        }

        ~GamesPageViewModel() 
        {
            Debug.WriteLine("GamesPageViewModel finalized!");
        }

        private async Task LoadProcessesAsync()
        {
            var processList = await _processService.LoadAllProcessesAsync();

            if (processList != null)
            {
                Processes = new ObservableCollection<ProcessDataModel>(processList);
            }
        }

        private async Task LoadGamesAsync()
        {
            Games = new ObservableCollection<GamesModel>(await _gamesService.LoadAllGamesAsync());
        }

        private async Task LoadOverlaysAsync()
        {
            Overlays = new ObservableCollection<OverlayDataModel>(await _overlayService.LoadAllOverlaysAsync());
        }

        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////

        partial void OnSelectedProcessChanged(ProcessDataModel value)
        {
            if (value != null)
            {
                GameName = value.Name;
                GameExecutablePath = value.Path;
            }
        }

        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////


        [RelayCommand]
        private void RefreshProcesses()
        {
            Processes = null;
            LoadProcessesAsync();
        }

        [RelayCommand]
        private async void MakeNewGame()
        {
            if (string.IsNullOrEmpty(GameName))
            {
                return;
            }
            if (string.IsNullOrEmpty(GameExecutablePath))
            {
                return;
            }
            if (_selectedOverlay == null)
            {
                return;
            }

            var newGame = new GamesModel()
            {
                Name = GameName,
                OverlayName = _selectedOverlay.Name,
                ExecutablePath = GameExecutablePath
            };
            SelectedProcess = null;
            SelectedOverlay = null;
            GameExecutablePath = string.Empty;
            GameName = string.Empty;

            await _gamesService.SaveGameAsync(newGame);
            LoadGamesAsync();
        }

        [RelayCommand]
        private void EditGame()
        {
            if (SelectedGame == null)
            {
                return;
            }

            GameName = SelectedGame.Name;
            GameExecutablePath = SelectedGame.ExecutablePath;
            SelectedOverlay = Overlays.FirstOrDefault(o => o.Name.Equals(SelectedGame.OverlayName, StringComparison.OrdinalIgnoreCase));
            SelectedProcess = null;
        }

        [RelayCommand]
        private void DeleteGame()
        {
            if (SelectedGame == null)
            {
                return;
            }

            _gamesService.DeleteGameAsync(SelectedGame);
            LoadGamesAsync();
        }

        [RelayCommand]
        private void GetExecutableFromFile(IEnumerable<IStorageFile> files)
        {
            if (files == null || !files.Any())
            {
                return;
            }

            var file = files.FirstOrDefault();
            if (file != null && FileHandlerUtil.IsValidExecutablePath(file.TryGetLocalPath()))
            {
                GameExecutablePath = file.Path.AbsolutePath;
                GameName = FileHandlerUtil.GetFileName(file.Name);
                var foundProcess = Processes.FirstOrDefault(p => p.Name.Equals(file.Name, StringComparison.OrdinalIgnoreCase) || p.Path.Equals(file.Path.LocalPath, StringComparison.OrdinalIgnoreCase));
                if (foundProcess != null)
                {
                    SelectedProcess = foundProcess;
                }
                else
                {
                    SelectedProcess = null;
                }
            }
        }
    }
}