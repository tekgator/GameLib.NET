using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Epic.Model;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Epic;

[Guid("282B9BB6-54CA-4293-83CF-6F1134CDEEC6")]
[Export(typeof(ILauncher))]
public class EpicLauncher : ILauncher
{
    private IEnumerable<EpicGame>? _gameList;

    public EpicLauncher()
    {
        ClearCache();
    }

    #region Interface implementations
    public Guid Id => GetType().GUID;

    public string Name => "Epic Games";

    public Image Logo => Properties.Resources.Logo128px;

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(ExecutablePath);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames(CancellationToken cancellationToken = default)
    {
        _gameList ??= EpicGameFactory.GetGames(Id, cancellationToken);
        return _gameList;
    }

    public bool Start() => IsRunning || ProcessUtil.StartProcess(ExecutablePath);

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

        ExecutablePath = GetExecutable() ?? string.Empty;
        if (!string.IsNullOrEmpty(ExecutablePath))
        {
            Executable = Path.GetFileName(ExecutablePath);
            InstallDir = Path.GetDirectoryName(ExecutablePath) ?? string.Empty;
            IsInstalled = File.Exists(ExecutablePath);
        }
    }
    #endregion

    #region Private methods
    private static string? GetExecutable()
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