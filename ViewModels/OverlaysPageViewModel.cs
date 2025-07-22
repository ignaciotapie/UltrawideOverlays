using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlaysPageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _overlaysPageName = "Overlays Page";

        [ObservableProperty]
        private OverlayDataModel? _selectedOverlay;

        [ObservableProperty]
        private int _selectedOverlayIndex;

        [ObservableProperty]
        private string _searchBoxText;

        private readonly WindowFactory? factory;
        private readonly OverlayDataService? overlayDataService;

        public ObservableCollection<OverlayDataModel> Overlays { get; }

        /// <summary>
        /// Design-only constructor
        /// </summary>
        public OverlaysPageViewModel()
        {
            Page = Enums.ApplicationPageViews.OverlaysPage;
            PageName = "Overlays";

            Overlays = [];

            if (Design.IsDesignMode)
            {
                Overlays.Add(new OverlayDataModel("Overlay 1", "Path 1"));
                Overlays.Add(new OverlayDataModel("Overlay 2", "Path 2"));
                Overlays.Add(new OverlayDataModel("Overlay 3", "Path 3"));
            }
        }

        ~OverlaysPageViewModel()
        {
            Debug.WriteLine("OverlaysPageViewModel finalized!");
        }

        public OverlaysPageViewModel(OverlayDataService service, WindowFactory WFactory)
        {
            Page = Enums.ApplicationPageViews.OverlaysPage;
            PageName = "Overlays";

            Overlays = new ObservableCollection<OverlayDataModel>();
            factory = WFactory;
            overlayDataService = service;

            LoadOverlaysAsync();
        }

        partial void OnSearchBoxTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                // If the search box is empty, show all overlays
                LoadOverlaysAsync();
            }
            else
            {
                // Filter overlays based on the search text
                var filteredOverlays = new ObservableCollection<OverlayDataModel>(
                    Overlays.Where(o => o.Name.Contains(value, StringComparison.OrdinalIgnoreCase)));

                Overlays.Clear();
                foreach (var overlay in filteredOverlays)
                {
                    Overlays.Add(overlay);
                }

                // Reset selected overlay if no overlays match
                if (Overlays.Count == 0)
                {
                    SelectedOverlay = null;
                    SelectedOverlayIndex = -1;
                }
            }
        }

        partial void OnSelectedOverlayChanged(OverlayDataModel? value)
        {
            if (value != null)
            {
                SelectedOverlayIndex = Overlays.IndexOf(value);
            }
            else
            {
                SelectedOverlayIndex = -1; // No overlay selected
            }
        }

        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////

        [RelayCommand]
        private void AddNewButton()
        {
            if (factory == null)
            {
                return;
            }

            var window = factory.CreateWindow(Enums.WindowViews.OverlayEditorWindow);
            window.Show();

            window.Closed += OverlayWindowClosed;
        }

        [RelayCommand]
        private void EditButton()
        {
            if (factory == null) return;
            if (SelectedOverlayIndex < 0 || SelectedOverlayIndex >= Overlays.Count)
            {
                return; // No overlay selected
            }

            var window = factory.CreateWindow(Enums.WindowViews.OverlayEditorWindow, Overlays[SelectedOverlayIndex]);
            window.Show();

            window.Closed += OverlayWindowClosed;
        }

        [RelayCommand]
        private void DeleteButton()
        {
            if (SelectedOverlayIndex < 0 || SelectedOverlayIndex >= Overlays.Count)
            {
                return; // No overlay selected
            }

            var overlayToDelete = Overlays[SelectedOverlayIndex];
            Overlays.RemoveAt(SelectedOverlayIndex);

            // Optionally, you can also delete the overlay from the database
            if (overlayDataService != null)
            {
                overlayDataService.DeleteOverlayAsync(overlayToDelete);
            }
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////

        private async void LoadOverlaysAsync()
        {
            try
            {
                var overlays = await overlayDataService.LoadAllOverlaysAsync();
                if (overlays != null)
                {
                    Overlays.Clear();
                    foreach (var overlay in overlays)
                    {
                        Overlays.Add(overlay);
                    }
                    //Default to first overlay
                    if (Overlays.Count > 0)
                    {
                        SelectedOverlayIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle error
                System.Diagnostics.Debug.WriteLine($"Error loading overlays: {ex.Message}");
            }
        }
        private void OverlayWindowClosed(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.Closed -= OverlayWindowClosed;
                window.DataContext = null;
            }

            PathToBitmapConverter.CleanCache();
            LoadOverlaysAsync();
        }
    }
}
