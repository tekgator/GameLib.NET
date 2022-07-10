using Gamelib.Core.Util;
using GameLib.Plugin.Gog.Model;
using Microsoft.Win32;
using System.Globalization;

namespace GameLib.Plugin.Gog;

internal static class GogGameFactory
{
    /// <summary>
    /// Get games installed for the GoG launcher
    /// </summary>
    public static IEnumerable<GogGame> GetGames(string launcherExecutable, CancellationToken cancellationToken = default)
    {
        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, @"SOFTWARE\GOG.com\Games", true);

        if (regKey is null)
            return Enumerable.Empty<GogGame>();

        return regKey.GetSubKeyNames()
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(gameId => LoadFromRegistry(launcherExecutable, gameId))
            .Where(game => game is not null)
            .ToList()!;
    }

    /// <summary>
    /// Load the GoG game registry entry into a <see cref="GogGame"/> object
    /// </summary>
    private static GogGame? LoadFromRegistry(string launcherExecutable, string gameId)
    {
        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, $@"SOFTWARE\GOG.com\Games\{gameId}");
        if (regKey is null)
            return null;

        var game = new GogGame()
        {
            Id = (string)regKey.GetValue("gameID", string.Empty)!,
            Name = (string)regKey.GetValue("gameName", string.Empty)!,
            ExecutablePath = (string)regKey.GetValue("exe", string.Empty)!,
            Executable = (string)regKey.GetValue("exeFile", string.Empty)!,
            WorkingDir = (string)regKey.GetValue("workingDir", string.Empty)!,
            InstallDate = DateTime.TryParseExact(
                (string)regKey.GetValue("INSTALLDATE", string.Empty)!, "yyyy-MM-dd HH:mm:ss",
                null, DateTimeStyles.AssumeLocal,
                out DateTime tmpInstallDate) ? tmpInstallDate : DateTime.MinValue,

            BuildId = (string)regKey.GetValue("BUILDID", string.Empty)!,
            DependsOn = (string)regKey.GetValue("dependsOn", string.Empty)!,
            Dlc = (string)regKey.GetValue("DLC", string.Empty)!,
            InstallerLanguage = (string)regKey.GetValue("installer_language", string.Empty)!,
            LangCode = (string)regKey.GetValue("lang_code", string.Empty)!,
            Language = (string)regKey.GetValue("language", string.Empty)!,
            LaunchCommand = (string)regKey.GetValue("launchCommand", string.Empty)!,
            LaunchParam = (string)regKey.GetValue("launchParam", string.Empty)!,
            Path = (string)regKey.GetValue("path", string.Empty)!,
            ProductId = (string)regKey.GetValue("productID", string.Empty)!,
            StartMenu = (string)regKey.GetValue("startMenu", string.Empty)!,
            StartMenuLink = (string)regKey.GetValue("startMenuLink", string.Empty)!,
            SupportLink = (string)regKey.GetValue("supportLink", string.Empty)!,
            UninstallCommand = (string)regKey.GetValue("uninstallCommand", string.Empty)!,
            Version = (string)regKey.GetValue("ver", string.Empty)!,
        };

        if (string.IsNullOrEmpty(game.Id))
            return null;

        game.InstallDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;
        game.LaunchString = $"\"{launcherExecutable}\" /command=runGame /gameId={game.Id}";
        if (!string.IsNullOrEmpty(game.WorkingDir))
            game.LaunchString += $" /path=\"{game.WorkingDir}\"";

        return game;
    }
}
