using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using UltrawideOverlays.Decorator;
using UltrawideOverlays.Enums;
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
                        return new OverlayEditorWindowView
                        {
                            DataContext = new OverlayEditorWindowViewModel(provider.GetRequiredService<OverlayDataService>(), provider.GetRequiredService<SettingsDataService>(), provider.GetRequiredService<ImageWrapperDecorator>(), args)
                        };

                    case Enums.WindowViews.MainWindow:
                        var window = new MainWindow
                        {
                            DataContext = provider.GetRequiredService<MainWindowViewModel>()
                        };
                        window.Closing += MainWindow_Closing;
                        window.Closed += MainWindow_Closed;
                        return window;
                    case Enums.WindowViews.OverlayWindow:
                        return new OverlayView
                        {
                            DataContext = provider.GetRequiredService<OverlayViewModel>()
                        };
                    case Enums.WindowViews.MiniOverlayManager:
                        return new QuickOverlayWindowView
                        {
                            DataContext = provider.GetRequiredService<QuickOverlayWindowViewModel>()
                        };
                    default:
                        throw new ArgumentOutOfRangeException(nameof(windowEnum), windowEnum, null);
                }
            }
            return null;
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
            collection.AddSingleton<GeneralDataService>();

            collection.AddSingleton<ActivityDataService>();
            collection.AddSingleton<SettingsDataService>();
            collection.AddSingleton<ImageCacheService>();
            collection.AddSingleton<HotKeyService>();
            collection.AddSingleton<FocusMonitorService>();

            //Factories
            collection.AddTransient<PageFactory>();
            collection.AddTransient<WindowFactory>();
            collection.AddSingleton<ImageWrapperDecorator>();

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
            collection.AddTransient<AppViewModel>();
            collection.AddTransient<MainWindowViewModel>();
            //PageViewModels
            collection.AddTransient<HomePageViewModel>();
            collection.AddTransient<OverlaysPageViewModel>();
            collection.AddTransient<GamesPageViewModel>();
            collection.AddTransient<SettingsPageViewModel>();
            collection.AddTransient<OverlayEditorWindowViewModel>();
            //Overlay ViewModel
            collection.AddTransient<OverlayViewModel>();
            //Quick Overlay ViewModel
            collection.AddTransient<QuickOverlayWindowViewModel>();

            return collection.BuildServiceProvider();
        }
        public async override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                services = ConfigureServices();

                var minimizeToTray = await services.GetRequiredService<SettingsDataService>().LoadSettingAsync(SettingsNames.MinimizeToTray);
                var IsSilentStartUp = desktop.Args?.Contains(AutoStartUtil.SILENT_ARG);
                var MinimizeToTray = minimizeToTray == SettingsBoolValues.False;

                if (IsSilentStartUp == false || MinimizeToTray)
                {
                    desktop.MainWindow = services.GetRequiredService<WindowFactory>().CreateWindow(Enums.WindowViews.MainWindow, null);
                    if (desktop.MainWindow != null)
                    {
                        desktop.MainWindow.Show();
                    }
                }

                DataContext = services.GetRequiredService<AppViewModel>();
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


        ///////////////////////////////////////////
        /// TRAY ICON HANDLING
        ///////////////////////////////////////////
        private void Open_Click(object? sender, System.EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.MainWindow == null)
                {
                    desktop.MainWindow = services.GetRequiredService<WindowFactory>().CreateWindow(Enums.WindowViews.MainWindow, null);
                }
                if (!desktop.MainWindow.IsVisible)
                {
                    desktop.MainWindow.Show();
                }
            }
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                QuitApp();
            }
        }

        private async void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var settingsService = services.GetRequiredService<SettingsDataService>();

                if (await settingsService.LoadSettingAsync(SettingsNames.MinimizeToTray) == SettingsBoolValues.True)
                {
                    e.Cancel = true;
                    if (sender is Window mainWindow)
                    {
                        mainWindow.Hide();
                    }
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