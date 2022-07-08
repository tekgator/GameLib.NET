using Gamelib.Util;
using GameLib.Plugin.Ubisoft.Model;
using GameLib.Util;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Ubisoft;

[Guid("CE276C05-6CD1-4D99-9A5A-2E03ECFB6028")]
[Export(typeof(ILauncher))]
public class UbisoftLauncher : ILauncher
{
    private readonly LauncherOptions _launcherOptions;
    private List<UbisoftGame>? _gameList;
    private UbisoftCatalog? _localCatalog;

    [ImportingConstructor]
    public UbisoftLauncher(LauncherOptions? launcherOptions)
    {
        _launcherOptions = launcherOptions ?? new LauncherOptions();
        ClearCache();
    }

    #region Interface implementations
    public string Name => "Ubisoft Connect";

    public bool IsInstalled { get; private set; } = false;

    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames()
    {
        if (IsInstalled && _launcherOptions.LoadLocalCatalogData)
        {
            try
            {
                _localCatalog ??= new UbisoftCatalog(InstallDir!);
            }
            catch { /* ignored */ }
        }

        _gameList ??= UbisoftGameFactory.GetGames(InstallDir, _localCatalog);
        return _gameList;
    }

    public void ClearCache()
    {
        _gameList = null;
        _localCatalog = null;

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

    public bool Start() =>
        IsInstalled && (IsRunning || Process.Start(ExecutablePath!) is not null);

    public void Stop() => ProcessUtil.StopProcess(ExecutablePath);
    #endregion

    #region Private methods
    private static string? ObtainExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("uplay");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Ubisoft\Launcher", "InstallDir");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!string.IsNullOrEmpty(executablePath) && !PathUtil.IsExecutable(executablePath))
            executablePath = Path.Combine(executablePath, "upc.exe");

        if (!PathUtil.IsExecutable(executablePath))
            executablePath = null;

        return executablePath;
    }
    #endregion

}