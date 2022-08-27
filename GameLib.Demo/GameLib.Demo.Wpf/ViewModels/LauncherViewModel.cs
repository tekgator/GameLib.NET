using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLib.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GameLib.Demo.Wpf.ViewModels;

public partial class LauncherViewModel : ViewModelBase
{
    const string CrossImagePath = "/Resources/cross-color.png";
    const string CheckImagePath = "/Resources/check-color.png";

    [ObservableProperty]
    private ObservableCollection<ILauncher> _launchers = default!;

    [ObservableProperty]
    private ILauncher? _selectedLauncher;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _isInstalledLogo = CrossImagePath;

    [ObservableProperty]
    private string _isRunningLogo = CrossImagePath;

    public int InstalledGameCount
    {
        get
        {
            if (_selectedLauncher is null)
                return 0;

            return _selectedLauncher.GetGames().Count();
        }
    }

    public LauncherViewModel(LauncherManager launcherManager)
    {
        LoadData(launcherManager);
    }

    private async void LoadData(LauncherManager launcherManager)
    {
        IsLoading = true;
        IEnumerable<ILauncher> launchers = Enumerable.Empty<ILauncher>();

        await Task.Run(() =>
        {
            try
            {
                launchers = launcherManager.GetLaunchers();
                launcherManager.GetGames();
            }
            catch { /* ignore */ }
        });

        Launchers = new(launchers);
        SelectedLauncher = Launchers.FirstOrDefault();
        IsLoading = false;
    }

    partial void OnSelectedLauncherChanged(ILauncher? value)
    {
        switch (value)
        {
            case null:
                IsInstalledLogo = CrossImagePath;
                IsRunningLogo = CrossImagePath;
                break;
            default:
                IsInstalledLogo = value.IsInstalled ? CheckImagePath : CrossImagePath;
                IsRunningLogo = value.IsRunning ? CheckImagePath : CrossImagePath;
                break;
        }
        OnPropertyChanged(nameof(InstalledGameCount));
        RunLauncherCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    public void RefreshIsRunning()
    {
        OnPropertyChanged(nameof(SelectedLauncher));
    }

    [RelayCommand(CanExecute = nameof(RunLauncherCanExecute))]
    public void RunLauncher(string exePath)
    {
        if (string.IsNullOrEmpty(exePath))
            return;

        Process.Start(exePath);
    }

    public bool RunLauncherCanExecute() =>
        SelectedLauncher is not null && SelectedLauncher.IsInstalled && !SelectedLauncher.IsRunning;

    [RelayCommand]
    public void OpenPath()
    {
        switch (SelectedLauncher)
        {
            case not null when !string.IsNullOrEmpty(SelectedLauncher.ExecutablePath):
                Process.Start("explorer.exe", $"/select,\"{SelectedLauncher.ExecutablePath}\"");
                break;
            case not null when !string.IsNullOrEmpty(SelectedLauncher.InstallDir):
                Process.Start("explorer.exe", SelectedLauncher.InstallDir);
                break;
        }
    }

    [RelayCommand]
    public static void CopyToClipboard(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        Clipboard.SetText(text);
    }
}
