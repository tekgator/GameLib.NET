using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Gog.Model;
using Microsoft.Win32;
using System.Globalization;

namespace GameLib.Plugin.Gog;

internal static class GogGameFactory
{
    /// <summary>
    /// Get games installed for the GoG launcher
    /// </summary>
    public static IEnumerable<GogGame> GetGames(ILauncher launcher, CancellationToken cancellationToken = default)
    {
        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, @"SOFTWARE\GOG.com\Games", true);

        if (regKey is null)
        {
            return Enumerable.Empty<GogGame>();
        }

        return regKey.GetSubKeyNames()
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(gameId => LoadFromRegistry(launcher, gameId))
            .Where(game => game is not null)
            .Select(game => AddLauncherId(launcher, game!))
            .Select(game => AddExecutables(launcher, game!))
            .ToList()!;
    }

    /// <summary>
    /// Add launcher ID to Game
    /// </summary>
    private static GogGame AddLauncherId(ILauncher launcher, GogGame game)
    {
        game.LauncherId = launcher.Id;
        return game;
    }

    /// <summary>
    /// Find executables within the install directory
    /// </summary>
    private static GogGame AddExecutables(ILauncher launcher, GogGame game)
    {
        if (launcher.LauncherOptions.SearchExecutables)
        {
            var executables = PathUtil.GetExecutables(game.InstallDir);

            executables.AddRange(game.Executables);
            game.Executables = executables.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        return game;
    }

    /// <summary>
    /// Load the GoG game registry entry into a <see cref="GogGame"/> object
    /// </summary>
    private static GogGame? LoadFromRegistry(ILauncher launcher, string gameId)
    {
        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, $@"SOFTWARE\GOG.com\Games\{gameId}");
        if (regKey is null)
        {
            return null;
        }

        var game = new GogGame()
        {
            Id = (string)regKey.GetValue("gameID", string.Empty)!,
            Name = (string)regKey.GetValue("gameName", string.Empty)!,
            Executable = (string)regKey.GetValue("exe", string.Empty)!,
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
        {
            return null;
        }

        game.InstallDir = string.IsNullOrEmpty(game.Executable) ? string.Empty : Path.GetDirectoryName(game.Executable) ?? string.Empty;
        game.LaunchString = $"\"{launcher.Executable}\" /command=runGame /gameId={game.Id}";
        if (!string.IsNullOrEmpty(game.WorkingDir))
        {
            game.LaunchString += $" /path=\"{game.WorkingDir}\"";
        }

        return game;
    }
}
