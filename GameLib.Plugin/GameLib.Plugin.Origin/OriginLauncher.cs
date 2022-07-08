using Gamelib.Util;
using GameLib.Origin.Model;
using GameLib.Util;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameLib.Origin;

[Guid("54C9D299-107E-4990-894D-9DB402F81CA3")]
[Export(typeof(ILauncher))]
public class OriginLauncher : ILauncher
{
    private readonly LauncherOptions _launcherOptions;
    private List<OriginGame>? _gameList;

    [ImportingConstructor]
    public OriginLauncher(LauncherOptions? launcherOptions)
    {
        _launcherOptions = launcherOptions ?? new LauncherOptions();
        ClearCache();
    }

    #region Interface implementations
    public string Name => "Origin";

    public bool IsInstalled { get; private set; } = false;

    public bool IsRunning => ProcessUtil.IsProcessRunning(ExecutablePath);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames()
    {
        _gameList ??= OriginGameFactory.GetGames(_launcherOptions.QueryOnlineData);
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