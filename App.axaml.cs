using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using UltrawideOverlays.Factories;
using UltrawideOverlays.Services;
using UltrawideOverlays.ViewModels;
using UltrawideOverlays.Views;

namespace UltrawideOverlays
{
    public partial class App : Application
    {
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
                    default:
                        throw new ArgumentOutOfRangeException(nameof(windowEnum), windowEnum, null);
                }
            }
            return null;
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
                // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
                DisableAvaloniaDataAnnotationValidation();

                ServiceProvider services = ConfigureServices();
                desktop.MainWindow = new MainWindow
                {
                    DataContext = services.GetRequiredService<MainWindowViewModel>()
                };
            }

            base.OnFrameworkInitializationCompleted();
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

            collection.AddSingleton<PageFactory>();
            collection.AddSingleton<SubviewFactory>();
            collection.AddSingleton<WindowFactory>();

            //ViewModel factories
            collection.AddTransient<Func<Enums.Subviews, ViewModelBase>>(x => vmName => vmName switch
            {
                Enums.Subviews.AddGameSubview => x.GetRequiredService<HomePageViewModel>(),
                _ => throw new InvalidOperationException($"No ViewModel found for {vmName}")
            });

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
            collection.AddSingleton<MainWindowViewModel>();
            //PageViewModels
            collection.AddTransient<HomePageViewModel>();
            collection.AddTransient<OverlaysPageViewModel>();
            collection.AddTransient<GamesPageViewModel>();
            collection.AddTransient<SettingsPageViewModel>();
            collection.AddTransient<OverlayEditorWindowViewModel>();
            //SubViewModels

            return collection.BuildServiceProvider();
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
    }
}