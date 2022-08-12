using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Steam.Model;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Steam;

[Guid("5BB973D0-BF3D-4C3E-98B2-41AEFCB1506A")]
[Export(typeof(ILauncher))]
public class SteamLauncher : ILauncher
{
    private readonly LauncherOptions _launcherOptions;
    private IEnumerable<SteamLibrary>? _libraryList;
    private IEnumerable<SteamGame>? _gameList;
    private SteamCatalog? _localCatalog;

    [ImportingConstructor]
    public SteamLauncher(LauncherOptions? launcherOptions)
    {
        _launcherOptions = launcherOptions ?? new LauncherOptions();
        ClearCache();
    }

    #region Interface implementations
    public string Name => "Steam";

    public Image SmallLogo => Properties.Resources.Logo32px;

    public Image LargeLogo => Properties.Resources.Logo512px;

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(ExecutablePath);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames(CancellationToken cancellationToken = default)
    {
        if (IsInstalled && _launcherOptions.LoadLocalCatalogData)
        {
            try
            {
                _localCatalog ??= new SteamCatalog(InstallDir);
            }
            catch { /* ignored */ }
        }

        _gameList ??= SteamGameFactory.GetGames(GetLibraries(), _localCatalog);
        return _gameList;
    }

    public bool Start() => IsRunning || ProcessUtil.StartProcess(ExecutablePath);

    public void Stop()
    {
        if (IsRunning)
            Process.Start(ExecutablePath, "-shutdown");
    }

    public void ClearCache()
    {
        _libraryList = null;
        _gameList = null;
        _localCatalog = null;

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

    #region Public methods
    public IEnumerable<SteamLibrary> GetLibraries()
    {
        _libraryList ??= SteamLibraryFactory.GetLibraries(InstallDir);
        return _libraryList;
    }
    #endregion

    #region Private methods
    private static string? GetExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("steam");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.CurrentUser, @"Software\Valve\Steam", "SteamExe");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.CurrentUser, @"Software\Valve\Steam", "SteamPath");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"Software\Valve\Steam", "InstallPath");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!string.IsNullOrEmpty(executablePath) && !PathUtil.IsExecutable(executablePath))
            executablePath = Path.Combine(executablePath, "steam.exe");

        if (!PathUtil.IsExecutable(executablePath))
            executablePath = null;

        return executablePath;
    }
    #endregion

}