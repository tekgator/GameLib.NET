using Gamelib.Util;
using GameLib.Plugin.Epic.Model;
using GameLib.Util;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Epic;

[Guid("282B9BB6-54CA-4293-83CF-6F1134CDEEC6")]
[Export(typeof(ILauncher))]
public class EpicLauncher : ILauncher
{
    private List<EpicGame>? _gameList;

    public EpicLauncher()
    {
        ClearCache();
    }

    #region Interface implementations
    public string Name => "Epic Games";

    public bool IsInstalled { get; private set; } = false;

    public bool IsRunning => ProcessUtil.IsProcessRunning(ExecutablePath);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames()
    {
        _gameList ??= EpicGameFactory.GetGames();
        return _gameList;
    }

    public bool Start() =>
        IsInstalled && (IsRunning || Process.Start(ExecutablePath!) is not null);

    public void Stop()
    {
        if (IsRunning)
            ProcessUtil.StopProcess(ExecutablePath);
    }

    public void ClearCache()
    {
        _gameList = null;

        ExecutablePath = string.Empty;
        Executable = string.Empty;
        InstallDir = string.Empty;
        IsInstalled = false;

        ExecutablePath = ObtainExecutable() ?? string.Empty;
        if (!string.IsNullOrEmpty(ExecutablePath))
        {
            Executable = Path.GetFileName(ExecutablePath);
            InstallDir = Path.GetDirectoryName(ExecutablePath) ?? string.Empty;
            IsInstalled = File.Exists(ExecutablePath);
        }
    }
    #endregion

    #region Private methods
    private static string? ObtainExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("com.epicgames.launcher");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.CurrentUser, @"Software\Epic Games\EOS", "ModSdkCommand");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!PathUtil.IsExecutable(executablePath))
            executablePath = null;

        return executablePath;
    }
    #endregion
}