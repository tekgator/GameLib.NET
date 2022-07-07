using Gamelib.Util;
using GameLib.Plugin.Steam.Model;
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
    private string? _executablePath = null;
    private List<SteamLibrary>? _libraryList = null;
    private List<SteamGame>? _gameList = null;
    private SteamCatalogue? _localCatalogue = null;

    [ImportingConstructor]
    public SteamLauncher(LauncherOptions? launcherOptions)
    {
        _launcherOptions = launcherOptions ?? new LauncherOptions();
    }

    #region Interface implementations
    public string Name => "Steam";

    public bool IsInstalled =>
        !string.IsNullOrEmpty(ExecutablePath) &&
        File.Exists(ExecutablePath);

    public bool IsRunning =>
        !string.IsNullOrEmpty(ExecutablePath) &&
        Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Executable)).Where(p => !p.HasExited).Any();

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
                _localCatalogue ??= new SteamCatalogue(InstallDir!);

            _gameList ??= SteamGameFactory.GetGames((IEnumerable<SteamLibrary>)Libraries, _localCatalogue);
            return _gameList;
        }
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
        _executablePath = null;
        _libraryList = null;
        _gameList = null;
        _localCatalogue = null;
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

    private List<SteamLibrary> GetLibraries()
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

        libraryList.AddRange(deserializedLibraries.
            Select(lib =>
            {
                lib.Value.Path = PathUtil.Sanitize(lib.Value.Path) ?? string.Empty;
                return lib.Value.SteamLibraryBuilder();
            })

        );

        return libraryList;
    }
    #endregion

}