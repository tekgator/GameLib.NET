using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Ubisoft.Model;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Ubisoft;

[Guid("CE276C05-6CD1-4D99-9A5A-2E03ECFB6028")]
[Export(typeof(ILauncher))]
public class UbisoftLauncher : ILauncher
{
    private readonly LauncherOptions _launcherOptions;
    private IEnumerable<UbisoftGame>? _gameList;
    private UbisoftCatalog? _localCatalog;

    [ImportingConstructor]
    public UbisoftLauncher(LauncherOptions? launcherOptions)
    {
        _launcherOptions = launcherOptions ?? new LauncherOptions();
        ClearCache();
    }

    #region Interface implementations
    public string Name => "Ubisoft Connect";

    public Image Icon => Properties.Resources.Logo32px;

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames(CancellationToken cancellationToken = default)
    {
        var installDir = InstallDir;

        if (!string.IsNullOrEmpty(InstallDir) && _launcherOptions.LoadLocalCatalogData)
        {
            try
            {
                _localCatalog ??= new UbisoftCatalog(installDir);
            }
            catch { /* ignored */ }
        }

        _gameList ??= UbisoftGameFactory.GetGames(_localCatalog, cancellationToken);
        return _gameList ?? Enumerable.Empty<UbisoftGame>();
    }

    public void ClearCache()
    {
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

    public bool Start() => IsRunning || ProcessUtil.StartProcess(ExecutablePath);

    public void Stop() => ProcessUtil.StopProcess(ExecutablePath);
    #endregion

    #region Private methods
    private static string? GetExecutable()
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