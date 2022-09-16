using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLib.Core;
using GameLib.Demo.Wpf.Util;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GameLib.Demo.Wpf.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    const string CrossImagePath = "/Resources/cross-color.png";
    const string CheckImagePath = "/Resources/check-color.png";

    private readonly LauncherManager _launcherManager;

    [ObservableProperty]
    private ObservableCollection<IGame> _games = default!;

    [ObservableProperty]
    private IGame? _selectedGame;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _noGameFound = false;

    [ObservableProperty]
    private string _isRunningLogo = CrossImagePath;

    [ObservableProperty]
    private string? _launcherName;

    public GameViewModel(LauncherManager launcherManager)
    {
        _launcherManager = launcherManager;
        LoadData();
    }

    private async void LoadData()
    {
        IsLoading = true;
        IEnumerable<IGame> games = Enumerable.Empty<IGame>();

        await Task.Run(() =>
        {
            try
            {
                games = _launcherManager.GetAllGames().OrderBy(g => g.Name);
            }
            catch { /* ignore */ }
        });

        Games = new(games);
        SelectedGame = Games.FirstOrDefault();
        IsLoading = false;
        if (!Games.Any())
        {
            NoGameFound = true;
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        switch (e.PropertyName)
        {
            case nameof(SelectedGame):
                switch (_selectedGame)
                {
                    case null:
                        LauncherName = string.Empty;
                        IsRunningLogo = CrossImagePath;
                        break;
                    default:
                        LauncherName = _launcherManager.GetLaunchers().First(l => l.Id == _selectedGame.LauncherId).Name;
                        IsRunningLogo = _selectedGame.IsRunning ? CheckImagePath : CrossImagePath;
                        break;
                }
                break;
        }
    }

    [RelayCommand]
    public void RefreshGameIsRunning()
    {
        OnPropertyChanged(nameof(SelectedGame));
    }

    [RelayCommand]
    public static void RunGame(IGame? game)
    {
        if (game is null)
        {
            return;
        }

        if (game.IsRunning)
        {
            FocusUtil.FocusProcess(Path.GetFileNameWithoutExtension(game.Executable));
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = game.Executable,
                WorkingDirectory = game.WorkingDir
            });
        }
        catch { /* ignore */ }
    }

    [RelayCommand]
    public static void RunLaunchString(IGame? game)
    {
        if (game is null)
        {
            return;
        }

        if (game.IsRunning)
        {
            FocusUtil.FocusProcess(Path.GetFileNameWithoutExtension(game.Executable));
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = game.LaunchString
            });
        }
        catch { /* ignore */ }
    }

    [RelayCommand]
    public static void OpenPath(string? path)
    {
        if (!string.IsNullOrEmpty(path))
        {
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else
            {
                Process.Start("explorer.exe", path);
            }
        }
    }

    [RelayCommand]
    public static void CopyToClipboard(object? obj)
    {
        var copyText = string.Empty;

        switch (obj)
        {
            case string text:
                copyText = text;
                break;
            case IEnumerable<string> list:
                copyText = string.Join("\n", list);
                break;
        }

        if (string.IsNullOrEmpty(copyText))
        {
            return;
        }
        Clipboard.SetText(copyText);
    }
}