using Gamelib.Util;
using GameLib.Plugin.Gog.Model;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Gog;

[Guid("02BBDAB7-68D7-4CE1-9044-934DFEC6CF17")]
[Export(typeof(ILauncher))]
public class GogLauncher : ILauncher
{
    private string? _executablePath = null;
    private List<GogLibrary>? _libraryList = null;
    private List<GogGame>? _gameList = null;


    #region Interface implementations
    public string Name => "GOG Galaxy";

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
            _gameList ??= GetGames();
            return _gameList;
        }
    }

    public void ClearCache()
    {
        _executablePath = null;
        _libraryList = null;
        _gameList = null;
    }

    public bool Start() =>
        IsInstalled && (IsRunning || Process.Start(ExecutablePath!) is not null);

    public void Stop()
    {
        if (IsRunning)
            Process.Start(ExecutablePath!, "/command=shutdown");
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

    public static List<GogGame> GetGames()
    {
        List<GogGame> games = new();

        var executable = GetExecutable();
        if (string.IsNullOrEmpty(executable))
            return games;

        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, @"SOFTWARE\GOG.com\Games", true);

        if (regKey is null)
            return games;

        foreach (var regKeyGameId in regKey!.GetSubKeyNames())
        {
            using var regKeyGame = regKey.OpenSubKey(regKeyGameId);
            if (regKeyGame is null)
                continue;

            var game = new GogGame()
            {
                GameId = (string)regKeyGame.GetValue("gameID", string.Empty)!,
                GameName = (string)regKeyGame.GetValue("gameName", string.Empty)!,
                InstallDir = "todo",
                ExecutablePath = (string)regKeyGame.GetValue("exe", string.Empty)!,
                Executable = (string)regKeyGame.GetValue("exeFile", string.Empty)!,
                WorkingDir = (string)regKeyGame.GetValue("workingDir", string.Empty)!,
                InstallDate = DateTime.TryParseExact(
                    (string)regKeyGame.GetValue("INSTALLDATE", string.Empty)!, "yyyy-MM-dd HH:mm:ss",
                    null, DateTimeStyles.AssumeLocal,
                    out DateTime tmpInstallDate) ? tmpInstallDate : DateTime.MinValue,

                BuildId = (string)regKeyGame.GetValue("BUILDID", string.Empty)!,
                DependsOn = (string)regKeyGame.GetValue("dependsOn", string.Empty)!,
                Dlc = (string)regKeyGame.GetValue("DLC", string.Empty)!,
                InstallerLanguage = (string)regKeyGame.GetValue("installer_language", string.Empty)!,
                LangCode = (string)regKeyGame.GetValue("lang_code", string.Empty)!,
                Language = (string)regKeyGame.GetValue("language", string.Empty)!,
                LaunchCommand = (string)regKeyGame.GetValue("launchCommand", string.Empty)!,
                LaunchParam = (string)regKeyGame.GetValue("launchParam", string.Empty)!,
                Path = (string)regKeyGame.GetValue("path", string.Empty)!,
                ProductId = (string)regKeyGame.GetValue("productID", string.Empty)!,
                StartMenu = (string)regKeyGame.GetValue("startMenu", string.Empty)!,
                StartMenuLink = (string)regKeyGame.GetValue("startMenuLink", string.Empty)!,
                SupportLink = (string)regKeyGame.GetValue("supportLink", string.Empty)!,
                UninstallCommand = (string)regKeyGame.GetValue("uninstallCommand", string.Empty)!,
                Version = (string)regKeyGame.GetValue("ver", string.Empty)!,
            };

            if (string.IsNullOrEmpty(game.GameId))
                continue;

            game.InstallDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;
            game.LaunchString = $"\"{executable}\" /command=runGame /gameId={game.GameId}";
            if (!string.IsNullOrEmpty(game.WorkingDir))
                game.LaunchString += $" /path=\"{game.WorkingDir}\"";

            games.Add(game);
        }

        return games;
    }

    private List<GogLibrary> GetLibraries()
    {
        var games = Games;
        List<GogLibrary> libraryList = new();

        libraryList.AddRange(games
            .Select(p => PathUtil.Sanitize(p.InstallDir.ToLower())!)
            .Distinct()
            .Select(installDir => new GogLibrary() { Path = installDir }));

        return libraryList;
    }
    #endregion

}