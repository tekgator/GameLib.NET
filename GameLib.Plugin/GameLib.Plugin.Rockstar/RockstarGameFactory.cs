using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Rockstar.Model;
using Microsoft.Win32;

namespace GameLib.Plugin.Rockstar;

internal static class RockstarGameFactory
{
    /// <summary>
    /// Get games installed for the GoG launcher
    /// </summary>
    public static IEnumerable<RockstarGame> GetGames(ILauncher launcher, CancellationToken cancellationToken = default)
    {
        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, @"SOFTWARE\Rockstar Games", true);

        if (regKey is null)
        {
            return Enumerable.Empty<RockstarGame>();
        }

        return regKey.GetSubKeyNames()
            //.AsParallel()
            //.WithCancellation(cancellationToken)
            .Select(gameId => LoadFromRegistry(launcher, gameId))
            .Where(game => game is not null)
            .Select(game => AddLauncherId(launcher, game!))
            .ToList()!;
    }

    /// <summary>
    /// Add launcher ID to Game
    /// </summary>
    private static RockstarGame AddLauncherId(ILauncher launcher, RockstarGame game)
    {
        game.LauncherId = launcher.Id;
        return game;
    }

    /// <summary>
    /// Load the GoG game registry entry into a <see cref="GogGame"/> object
    /// </summary>
    private static RockstarGame? LoadFromRegistry(ILauncher launcher, string gameId)
    {
        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, $@"SOFTWARE\Rockstar Games\{gameId}");
        if (regKey is null ||
            gameId.Equals("Launcher", StringComparison.OrdinalIgnoreCase) ||
            gameId.Equals("Rockstar Games Social Club", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrEmpty(regKey.GetValue("InstallFolder", string.Empty) as string))
        {
            return null;
        }

        var game = new RockstarGame()
        {
            Id = gameId,
            Name = gameId,
            InstallDir = (string)regKey.GetValue("InstallFolder", string.Empty)!,
            Version = (string)regKey.GetValue("Version", string.Empty)!,
        };

        if (string.IsNullOrEmpty(game.Id))
        {
            return null;
        }

        if (launcher.LauncherOptions.SearchGameConfigStore)
        {
            game.ExecutablePath = RegistryUtil.SearchGameConfigStore(game.InstallDir) ?? string.Empty;
        }

        game.WorkingDir = game.InstallDir;
        game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;

        if (!string.IsNullOrEmpty(game.ExecutablePath))
        {
            game.Executable = Path.GetFileName(game.ExecutablePath);
        }

        game.LaunchString = $"\"{launcher.ExecutablePath}\" -launchTitleInFolder \"{game.InstallDir}\"";

        return game;
    }
}
