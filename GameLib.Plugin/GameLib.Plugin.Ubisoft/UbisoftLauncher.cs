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
    private string? _executablePath = null;
    private List<UbisoftLibrary>? _libraryList = null;
    private List<UbisoftGame>? _gameList = null;
    private UbisoftCatalogue? _localCatalogue = null;

    [ImportingConstructor]
    public UbisoftLauncher(LauncherOptions? launcherOptions)
    {
        _launcherOptions = launcherOptions ?? new LauncherOptions();
    }

    #region Interface implementations
    public string Name => "Ubisoft Connect";

    public bool IsInstalled =>
        !string.IsNullOrEmpty(ExecutablePath) &&
        File.Exists(ExecutablePath);

    public bool IsRunning =>
        ProcessUtil.IsProcessRunning(Executable);

    public string? InstallDir =>
        Path.GetDirectoryName(ExecutablePath);

    public string? ExecutablePath
    {
        get
        {
            _executablePath ??= GetExecutable();
            return _executablePath;
        }
    }

    public string? Executable =>
        Path.GetFileName(ExecutablePath);

    public IEnumerable<ILibrary> Libraries
    {
        get
        {
            _libraryList ??= GetLibraries();
            return _libraryList;
        }
    }

    public IEnumerable<IGame> Games
    {
        get
        {
            if (IsInstalled && _launcherOptions.LoadLocalCatalogueData)
            {
                try
                {
                    _localCatalogue ??= new UbisoftCatalogue(InstallDir!);
                }
                catch { }
            }

            _gameList ??= UbisoftGameFactory.GetGames(InstallDir, _localCatalogue);
            return _gameList;
        }
    }

    public void ClearCache()
    {
        _executablePath = null;
        _libraryList = null;
        _gameList = null;
        _localCatalogue = null;
    }

    public bool Start() =>
        IsInstalled && (IsRunning || Process.Start(ExecutablePath!) is not null);

    public void Stop() =>
        ProcessUtil.StopProcess(ExecutablePath);
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

    private List<UbisoftLibrary> GetLibraries()
    {
        var games = Games;
        List<UbisoftLibrary> libraryList = new();

        libraryList.AddRange(games
            .Select(p => PathUtil.Sanitize(Path.GetDirectoryName(p.InstallDir.ToLower())) ?? string.Empty)
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct()
            .Select(installDir => new UbisoftLibrary() { Path = installDir }));

        return libraryList;
    }
    #endregion



}