using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Origin.Model;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Origin;

[Guid("54C9D299-107E-4990-894D-9DB402F81CA3")]
[Export(typeof(ILauncher))]
public class OriginLauncher : ILauncher
{
    private readonly LauncherOptions _launcherOptions;
    private IEnumerable<OriginGame>? _gameList;

    [ImportingConstructor]
    public OriginLauncher(LauncherOptions? launcherOptions)
    {
        _launcherOptions = launcherOptions ?? new LauncherOptions();
        ClearCache();
    }

    #region Interface implementations
    public string Name => "Origin";

    public Image Icon => Properties.Resources.Logo32px;

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(ExecutablePath);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames(CancellationToken cancellationToken = default)
    {
        _gameList ??= OriginGameFactory.GetGames(_launcherOptions.QueryOnlineData, _launcherOptions.OnlineQueryTimeout, cancellationToken);
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

        executablePath ??= RegistryUtil.GetShellCommand("origin");
        executablePath ??= RegistryUtil.GetShellCommand("origin2");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Origin", "ClientPath");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Origin", "OriginPath");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!PathUtil.IsExecutable(executablePath))
            executablePath = null;

        return executablePath;
    }
    #endregion
}