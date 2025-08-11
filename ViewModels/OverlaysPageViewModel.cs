using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using UltrawideOverlays.Decorator;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;
using UltrawideOverlays.Wrappers;

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
        private OverlayWrapper? _selectedOverlayImage;

        [ObservableProperty]
        private string _searchBoxText;

        private WindowFactory? WindowFactory;
        private OverlayDataService? OverlayDataService;
        private ImageWrapperDecorator? WrapperDecorator;

        public ObservableCollection<OverlayDataModel> Overlays { get; }

        ///////////////////////////////////////////
        /// CONSTRUCTOR
        ///////////////////////////////////////////

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
            Dispose();
        }

        ///////////////////////////////////////////
        /// PUBLIC FUNCTIONS
        ///////////////////////////////////////////

        public OverlaysPageViewModel(OverlayDataService service, WindowFactory WFactory, ImageWrapperDecorator wrapperDecorator, ImageCacheService cacheService)
        {
            Page = Enums.ApplicationPageViews.OverlaysPage;
            PageName = "Overlays";

            Overlays = new ObservableCollection<OverlayDataModel>();
            WindowFactory = WFactory;
            OverlayDataService = service;
            WrapperDecorator = wrapperDecorator;

            LoadOverlaysAsync();
        }

        ///////////////////////////////////////////
        /// OVERRIDE FUNCTIONS
        ///////////////////////////////////////////

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
            if (SelectedOverlayImage != null)
            {
                SelectedOverlayImage.Dispose();
            }
            if (value != null)
            {
                SelectedOverlayIndex = Overlays.IndexOf(value);
                SelectedOverlayImage = WrapperDecorator?.CreateOverlayWrapper(value, value.Path);
            }
            else
            {
                SelectedOverlayIndex = -1; // No overlay selected
                SelectedOverlayImage = null;
            }
        }

        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////
        private async void LoadOverlaysAsync()
        {
            try
            {
                var overlays = await OverlayDataService.LoadAllOverlaysAsync();
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
                (window.DataContext as IDisposable)?.Dispose();
            }

            LoadOverlaysAsync();
        }

        ///////////////////////////////////////////
        /// COMMANDS
        ///////////////////////////////////////////

        [RelayCommand]
        private void AddNewButton()
        {
            if (WindowFactory == null)
            {
                return;
            }

            var window = WindowFactory.CreateWindow(Enums.WindowViews.OverlayEditorWindow);
            window.Show();

            window.Closed += OverlayWindowClosed;
        }

        [RelayCommand]
        private void EditButton()
        {
            if (WindowFactory == null) return;
            if (SelectedOverlayIndex < 0 || SelectedOverlayIndex >= Overlays.Count)
            {
                return; // No overlay selected
            }

            var window = WindowFactory.CreateWindow(Enums.WindowViews.OverlayEditorWindow, Overlays[SelectedOverlayIndex]);
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

            var wrapperToDelete = SelectedOverlayImage;
            var overlayToDelete = Overlays[SelectedOverlayIndex];

            Overlays.RemoveAt(SelectedOverlayIndex);
            wrapperToDelete.Dispose();

            if (OverlayDataService != null)
            {
                OverlayDataService.DeleteOverlayAsync(overlayToDelete);
            }
        }

        public override void Dispose()
        {
            if (SelectedOverlayImage != null)
            {
                SelectedOverlayImage.Dispose();
                SelectedOverlayImage = null;
            }

            SelectedOverlay = null;

            WindowFactory = null;
            OverlayDataService = null;
            WrapperDecorator = null;

            GC.SuppressFinalize(this);

            Debug.WriteLine("OverlaysPageViewModel finalized!");
        }
    }
}
