using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Linq;
using UltrawideOverlays.Converters;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Services;
using UltrawideOverlays.ViewModels;
using UltrawideOverlays.Views;

using Window = Avalonia.Controls.Window;


namespace UltrawideOverlays
{
    public partial class App : Application
    {
        private ServiceProvider services;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public Window? CreateWindow(IServiceProvider provider, Enums.WindowViews windowEnum, Object? args)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                switch (windowEnum)
                {
                    case Enums.WindowViews.OverlayEditorWindow:
                        var overlayService = provider.GetRequiredService<OverlayDataService>();
                        return new OverlayEditorWindowView
                        {
                            //This goes against DI principles, but making a whole new factory for just passing args to a single window is overkill
                            DataContext = new OverlayEditorWindowViewModel(overlayService, args)
                        };
                    case Enums.WindowViews.MainWindow:
                        var window = new MainWindow
                        {
                            DataContext = services.GetRequiredService<MainWindowViewModel>(),
                            ClosingBehavior = WindowClosingBehavior.OwnerAndChildWindows,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen
                        };
                        window.Closing += MainWindow_Closing;
                        return window;
                    case Enums.WindowViews.OverlayWindow:
                        return new OverlayView
                        {
                            DataContext = provider.GetRequiredService<OverlayViewModel>(),
                        };
                    default:
                        throw new ArgumentOutOfRangeException(nameof(windowEnum), windowEnum, null);
                }
            }
            return null;
        }

        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                e.Cancel = true; // Cancel the closing event to prevent the main window from closing immediately
                // Hide the main window instead of closing it
                if (sender is Window mainWindow)
                {
                    mainWindow.Hide();
                }

                PathToBitmapConverter.CleanCache();
            }
        }

        private ServiceProvider ConfigureServices()
        {
            ServiceCollection collection = new ServiceCollection();

            //DB provider non-lazy initialization (for async db initialization!)
            var dbProvider = new DatabaseProvider();
            collection.AddSingleton(dbProvider);
            //Data services
            collection.AddSingleton<OverlayDataService>();
            collection.AddSingleton<GamesDataService>();
            collection.AddSingleton<ProcessDataService>();
            collection.AddSingleton<ActivityDataService>();
            collection.AddSingleton<GeneralDataService>();

            var focusMonitorService = new FocusMonitorService();
            collection.AddSingleton(focusMonitorService);

            //Factories
            collection.AddSingleton<PageFactory>();
            collection.AddSingleton<WindowFactory>();

            //ViewModel factories
            collection.AddTransient<Func<Enums.ApplicationPageViews, PageViewModel>>(x => pageName => pageName switch
            {
                Enums.ApplicationPageViews.HomePage => x.GetRequiredService<HomePageViewModel>(),
                Enums.ApplicationPageViews.OverlaysPage => x.GetRequiredService<OverlaysPageViewModel>(),
                Enums.ApplicationPageViews.GamesPage => x.GetRequiredService<GamesPageViewModel>(),
                Enums.ApplicationPageViews.SettingsPage => x.GetRequiredService<SettingsPageViewModel>(),
                _ => throw new InvalidOperationException($"No ViewModel found for {pageName}")
            });

            collection.AddTransient<Func<Enums.WindowViews, object?, Window>>(x => (windowEnum, args) => CreateWindow(x, windowEnum, args));

            //Main Window
            collection.AddTransient<MainWindowViewModel>();
            //PageViewModels
            collection.AddTransient<HomePageViewModel>();
            collection.AddTransient<OverlaysPageViewModel>();
            collection.AddTransient<GamesPageViewModel>();
            collection.AddTransient<SettingsPageViewModel>();
            collection.AddTransient<OverlayEditorWindowViewModel>();
            //Overlay ViewModel
            collection.AddTransient<OverlayViewModel>();

            return collection.BuildServiceProvider();
        }
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                services = ConfigureServices();

                desktop.MainWindow = services.GetRequiredService<WindowFactory>().CreateWindow(Enums.WindowViews.MainWindow, null);
                services.GetRequiredService<WindowFactory>().CreateWindow(Enums.WindowViews.OverlayWindow, null);
            }
            base.OnFrameworkInitializationCompleted();
        }

        private void DisableAvaloniaDataAnnotationValidation()
        {
            // Get an array of plugins to remove
            var dataValidationPluginsToRemove =
                BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

            // remove each entry found
            foreach (var plugin in dataValidationPluginsToRemove)
            {
                BindingPlugins.DataValidators.Remove(plugin);
            }
        }

        private void Open_Click(object? sender, System.EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!desktop.MainWindow.IsVisible)
                {
                    desktop.MainWindow.Show();
                }
            }
        }

        private void Quit_Click(object? sender, System.EventArgs e)
        {
            QuitApp();
        }

        private void QuitApp()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                foreach (var window in desktop.Windows.ToList())
                {
                    window.Close();
                }

                desktop.Shutdown();
            }
        }
    }
}