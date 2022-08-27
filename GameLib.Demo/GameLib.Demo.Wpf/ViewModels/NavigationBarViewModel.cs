using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLib.Demo.Wpf.Models;
using GameLib.Demo.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace GameLib.Demo.Wpf.ViewModels;

public partial class NavigationBarViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<NavigationBarMenuItemModel> _menuItems = default!;

    [ObservableProperty]
    private NavigationBarMenuItemModel? _selectedMenuItem;

    public NavigationBarViewModel(
        INavigationService<HomeViewModel> homeNavigationService,
        INavigationService<LauncherViewModel> launcherNavigationService,
        INavigationService<GameViewModel> gameNavigationService)
    {
        MenuItems = new(new List<NavigationBarMenuItemModel>()
        {
            new NavigationBarMenuItemModel()
            {
                Text = "Home",
                ImageSource = "/Resources/home-white.png",
                Navigate = homeNavigationService.Navigate
            },
            new NavigationBarMenuItemModel()
            {
                Text = "Launchers",
                ImageSource = "/Resources/launcher-white.png",
                Navigate = launcherNavigationService.Navigate
            },
            new NavigationBarMenuItemModel()
            {
                Text = "Games",
                ImageSource = "/Resources/game-white.png",
                Navigate = gameNavigationService.Navigate
            },
        });

        SelectedMenuItem = MenuItems.FirstOrDefault();
    }

    partial void OnSelectedMenuItemChanged(NavigationBarMenuItemModel? value)
    {
        value?.Navigate();
    }

    [RelayCommand]
    public static void NavigateGithub(string address)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = address,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
