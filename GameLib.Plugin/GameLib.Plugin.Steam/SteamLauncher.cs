using Gamelib.Util;
using GameLib.Plugin.Steam.Model;
using GameLib.Util;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ValveKeyValue;

namespace GameLib.Plugin.Steam;

[Guid("5BB973D0-BF3D-4C3E-98B2-41AEFCB1506A")]
[Export(typeof(ILauncher))]
public class SteamLauncher : ILauncher
{
    private readonly LauncherOptions _launcherOptions;
    private List<SteamLibrary>? _libraryList;
    private List<SteamGame>? _gameList;
    private SteamCatalog? _localCatalog;

    [ImportingConstructor]
    public SteamLauncher(LauncherOptions? launcherOptions)
    {
        _launcherOptions = launcherOptions ?? new LauncherOptions();
        ClearCache();
    }

    #region Interface implementations
    public string Name => "Steam";

    public bool IsInstalled { get; private set; } = false;

    public bool IsRunning => ProcessUtil.IsProcessRunning(ExecutablePath);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> GetGames()
    {
        if (IsInstalled && _launcherOptions.LoadLocalCatalogData)
        {
            try
            {
                _localCatalog ??= new SteamCatalog(InstallDir);
            }
            catch { /* ignored */ }
        }

        _gameList ??= SteamGameFactory.GetGames(ObtainLibraries(), _localCatalog);
        return _gameList;
    }

    public bool Start() =>
        IsInstalled && (IsRunning || Process.Start(ExecutablePath!) is not null);

    public void Stop()
    {
        if (IsRunning)
            Process.Start(ExecutablePath!, "-shutdown");
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

        ExecutablePath = ObtainExecutable() ?? string.Empty;
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
        _libraryList ??= ObtainLibraries();
        return _libraryList;
    }
    #endregion

    #region Private methods
    private static string? ObtainExecutable()
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

    private List<SteamLibrary> ObtainLibraries()
    {
        List<SteamLibrary> libraryList = new();

        var installDir = InstallDir;
        if (string.IsNullOrEmpty(installDir))
            return libraryList;

        var libraryVdfPath = Path.Combine(installDir, "config", "libraryfolders.vdf");

        if (!File.Exists(libraryVdfPath))
            return libraryList;

        using var stream = File.OpenRead(libraryVdfPath);
        var serializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

        var deserializedLibraries = serializer.Deserialize<Dictionary<string, DeserializedSteamLibrary>>(stream);

        libraryList.AddRange(deserializedLibraries
            .Select(lib => lib.Value)
            .Select(value =>
            {
                value.Path = PathUtil.Sanitize(value.Path) ?? string.Empty;
                return value.SteamLibraryBuilder();
            })
        );

        return libraryList;
    }
    #endregion

}