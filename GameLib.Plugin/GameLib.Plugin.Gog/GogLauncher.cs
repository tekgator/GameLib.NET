using Gamelib.Util;
using GameLib.Plugin.Gog.Model;
using GameLib.Util;
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
    private List<GogGame>? _gameList = null;

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

    public IEnumerable<IGame> GetGames()
    {
        _gameList ??= ObtainGames();
        return _gameList;
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

    public bool Start() =>
        IsInstalled && (IsRunning || Process.Start(ExecutablePath!) is not null);

    public void Stop()
    {
        if (IsRunning)
            Process.Start(ExecutablePath!, "/command=shutdown");
    }
    #endregion

    #region Private methods
    private static string? ObtainExecutable()
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

    private List<GogGame> ObtainGames()
    {
        List<GogGame> games = new();

        var executable = Executable;
        if (string.IsNullOrEmpty(executable))
            return games;

        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, @"SOFTWARE\GOG.com\Games", true);

        if (regKey is null)
            return games;

        foreach (var regKeyGameId in regKey.GetSubKeyNames())
        {
            using var regKeyGame = regKey.OpenSubKey(regKeyGameId);
            if (regKeyGame is null)
                continue;

            var game = new GogGame()
            {
                GameId = (string)regKeyGame.GetValue("gameID", string.Empty)!,
                GameName = (string)regKeyGame.GetValue("gameName", string.Empty)!,
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
    #endregion

}