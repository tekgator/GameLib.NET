using CommunityToolkit.Mvvm.Input;
using System;
using System.Diagnostics;
using System.Windows;

namespace GameLib.Demo.Wpf.ViewModels;

public partial class NavigationBarViewModel : ViewModelBase
{
    [RelayCommand]
    public void NavigateHome()
    {

    }

    [RelayCommand]
    public void NavigateLauncher()
    {

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
