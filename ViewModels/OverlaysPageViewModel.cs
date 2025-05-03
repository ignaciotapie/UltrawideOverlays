using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Models;

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

        public OverlaysPageViewModel(IEnumerable<OverlayDataModel> overlays, WindowFactory WFactory)
        {
            PageName = Enums.ApplicationPageViews.OverlaysPage;
            Overlays = new ObservableCollection<OverlayDataModel>(overlays);
            factory = WFactory;
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
