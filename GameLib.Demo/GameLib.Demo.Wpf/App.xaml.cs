using GameLib.Demo.Wpf.Services;
using GameLib.Demo.Wpf.Store;
using GameLib.Demo.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace GameLib.Demo.Wpf;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddSingleton<NavigationStore>();

        services.AddSingleton<INavigationService<HomeViewModel>>(s => CreateHomeNavigationService(s));
        services.AddTransient<HomeViewModel>();

        services.AddSingleton<INavigationService<LauncherViewModel>>(s => CreateLauncherNavigationService(s));
        services.AddTransient<LauncherViewModel>();

        services.AddSingleton<INavigationService<GameViewModel>>(s => CreateGameNavigationService(s));
        services.AddTransient<GameViewModel>();

        services.AddSingleton<NavigationBarViewModel>(CreateNavigationBarViewModel);

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>(s => new MainWindow()
        {
            DataContext = s.GetRequiredService<MainViewModel>()
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        var initialNavigationService = _serviceProvider.GetRequiredService<INavigationService<HomeViewModel>>();
        initialNavigationService.Navigate();

        MainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow.Show();

        base.OnStartup(e);
    }

    private static INavigationService<HomeViewModel> CreateHomeNavigationService(IServiceProvider serviceProvider)
    {
        return new NavigationService<HomeViewModel>(
            serviceProvider.GetRequiredService<NavigationStore>(),
            () => serviceProvider.GetRequiredService<HomeViewModel>());
    }

    private static INavigationService<LauncherViewModel> CreateLauncherNavigationService(IServiceProvider serviceProvider)
    {
        return new NavigationService<LauncherViewModel>(
            serviceProvider.GetRequiredService<NavigationStore>(),
            () => serviceProvider.GetRequiredService<LauncherViewModel>());
    }

    private static INavigationService<GameViewModel> CreateGameNavigationService(IServiceProvider serviceProvider)
    {
        return new NavigationService<GameViewModel>(
            serviceProvider.GetRequiredService<NavigationStore>(),
            () => serviceProvider.GetRequiredService<GameViewModel>());
    }

    private static NavigationBarViewModel CreateNavigationBarViewModel(IServiceProvider serviceProvider)
    {
        return new NavigationBarViewModel(
            CreateHomeNavigationService(serviceProvider),
            CreateLauncherNavigationService(serviceProvider),
            CreateGameNavigationService(serviceProvider));
    }
}
