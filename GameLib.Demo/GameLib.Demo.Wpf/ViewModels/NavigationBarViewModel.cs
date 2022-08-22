using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLib.Demo.Wpf.Services;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace GameLib.Demo.Wpf.ViewModels;

public partial class NavigationBarViewModel : ViewModelBase
{
    private readonly INavigationService<HomeViewModel> _homeNavigationService;
    private readonly INavigationService<LauncherViewModel> _launcherNavigationService;

    [ObservableProperty]
    private ListViewItem? _selectedItem;

    public NavigationBarViewModel(
        INavigationService<HomeViewModel> homeNavigationService,
        INavigationService<LauncherViewModel> launcherNavigationService)
    {
        _homeNavigationService = homeNavigationService;
        _launcherNavigationService = launcherNavigationService;
    }

    partial void OnSelectedItemChanged(ListViewItem? value)
    {
        throw new NotImplementedException();
    }

    [RelayCommand]
    public void NavigateHome()
    {
        _homeNavigationService.Navigate();
    }

    [RelayCommand]
    public void NavigateLauncher()
    {
        _launcherNavigationService.Navigate();
    }

    [RelayCommand]
    public void NavigateGame()
    {

    }

    [RelayCommand]
    public void NavigateContribute()
    {

    }

    [RelayCommand]
    public void NavigateAbout()
    {

    }

    [RelayCommand]
    public void NavigateGithub()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/tekgator/GameLib.NET",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
