using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;
using UltrawideOverlays.Services;

namespace UltrawideOverlays.ViewModels
{
    public partial class OverlaysPageViewModel : PageViewModel
    {
        [ObservableProperty]
        private string _overlaysPageName = "Overlays Page";

        private readonly WindowFactory? factory;

        public ObservableCollection<OverlayDataModel>? Overlays { get; }

        /// <summary>
        /// Design-only constructor
        /// </summary>
        public OverlaysPageViewModel()
        {
            PageName = Enums.ApplicationPageViews.OverlaysPage;
            Overlays = [];

            if (Design.IsDesignMode)
            {
                Overlays.Add(new OverlayDataModel("Overlay 1", "Path 1"));
                Overlays.Add(new OverlayDataModel("Overlay 2", "Path 2"));
                Overlays.Add(new OverlayDataModel("Overlay 3", "Path 3"));
            }
        }

        public OverlaysPageViewModel(OverlayDataService service, WindowFactory WFactory)
        {
            PageName = Enums.ApplicationPageViews.OverlaysPage;
            Overlays = new ObservableCollection<OverlayDataModel>();
            factory = WFactory;

            LoadOverlaysAsync(service);
        }

        private async void LoadOverlaysAsync(OverlayDataService service)
        {
            try
            {
                var overlays = await service.LoadAllOverlaysAsync();
                if (overlays != null)
                {
                    foreach (var overlay in overlays)
                    {
                        Overlays.Add(overlay);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or handle error
                System.Diagnostics.Debug.WriteLine($"Error loading overlays: {ex.Message}");
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


        ///////////////////////////////////////////
        /// PRIVATE FUNCTIONS
        ///////////////////////////////////////////
        ///
        private void OverlayWindowClosed(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.Closed -= OverlayWindowClosed;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }
}
