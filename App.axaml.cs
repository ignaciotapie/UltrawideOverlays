using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using UltrawideOverlays.Converters;
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
                            DataContext = new OverlayEditorWindowViewModel(services.GetRequiredService<OverlayDataService>(), services.GetRequiredService<SettingsDataService>(), args)
                        };
                    case Enums.WindowViews.MainWindow:
                        var window = new MainWindow
                        {
                            DataContext = services.GetRequiredService<MainWindowViewModel>(),
                            ClosingBehavior = WindowClosingBehavior.OwnerAndChildWindows,
                            WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        };
                        window.Closing += MainWindow_Closing;
                        window.Closed += MainWindow_Closed;
                        return window;
                    case Enums.WindowViews.OverlayWindow:
                        return new OverlayView
                        {
                            DataContext = provider.GetRequiredService<OverlayViewModel>()
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
            collection.AddTransient<OverlayDataService>();
            collection.AddTransient<GamesDataService>();
            collection.AddTransient<ProcessDataService>();
            collection.AddTransient<ActivityDataService>();
            collection.AddTransient<GeneralDataService>();
            collection.AddSingleton<SettingsDataService>();
            //TODO: Global hotkey service...
            HotKeyService hkservice = new HotKeyService();
            collection.AddSingleton<HotKeyService>(hkservice);

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
            collection.AddSingleton<AppViewModel>();
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
        public async override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                services = ConfigureServices();

                var minimizeToTray = await services.GetRequiredService<SettingsDataService>().LoadSettingAsync(SettingsNames.MinimizeToTray);

                if (!desktop.Args.Contains(AutoStartUtil.SILENT_ARG) || minimizeToTray == SettingsBoolValues.False)
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

                    PathToCachedBitmapConverter.Instance.ClearCache();
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