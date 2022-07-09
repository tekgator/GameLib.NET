using Gamelib.Util;
using GameLib.Plugin.Gog.Model;
using GameLib.Util;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Gog;

[Guid("02BBDAB7-68D7-4CE1-9044-934DFEC6CF17")]
[Export(typeof(ILauncher))]
public class GogLauncher : ILauncher
{
    private IEnumerable<GogGame>? _gameList;

    public GogLauncher()
    {
        ClearCache();
    }

    #region Interface implementations
    public string Name => "GOG Galaxy";

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(ExecutablePath))
            _gameList ??= GogGameFactory.GetGames(ExecutablePath, cancellationToken);

        return _gameList ?? Enumerable.Empty<GogGame>();
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

    public bool Start() => IsRunning || ProcessUtil.StartProcess(ExecutablePath);

    public void Stop()
    {
        if (IsRunning)
            Process.Start(ExecutablePath, "/command=shutdown");
    }
    #endregion

    #region Private methods
    private static string? GetExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("goggalaxy");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\GOG.com\GalaxyClient\paths", "client");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!string.IsNullOrEmpty(executablePath) && !PathUtil.IsExecutable(executablePath))
            executablePath = Path.Combine(executablePath, RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\GOG.com\GalaxyClient", "clientExecutable") ?? string.Empty);

        if (!PathUtil.IsExecutable(executablePath))
            executablePath = null;

        return executablePath;
    }
    #endregion

}