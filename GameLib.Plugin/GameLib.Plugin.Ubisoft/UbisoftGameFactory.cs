using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Ubisoft.Model;
using Microsoft.Win32;

namespace GameLib.Plugin.Ubisoft;

internal static class UbisoftGameFactory
{
    /// <summary>
    /// Get games installed for the Ubisoft launcher
    /// </summary>
    public static IEnumerable<UbisoftGame> GetGames(ILauncher launcher, CancellationToken cancellationToken = default)
    {
        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, @"SOFTWARE\Ubisoft\Launcher\Installs", true);

        if (regKey is null)
        {
            return Enumerable.Empty<UbisoftGame>();
        }

        UbisoftCatalog? localCatalog = GetCatalog(launcher);

        return regKey.GetSubKeyNames()
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(LoadFromRegistry)
            .Where(game => game is not null)
            .Select(game => AddLauncherId(launcher, game!))
            .Select(game => AddCatalogData(game!, localCatalog))
            .ToList();
    }

    /// <summary>
    /// Add launcher ID to Game
    /// </summary>
    private static UbisoftGame AddLauncherId(ILauncher launcher, UbisoftGame game)
    {
        game.LauncherId = launcher.Id;
        return game;
    }

    /// <summary>
    /// Load steam local catalog data
    /// </summary>
    private static UbisoftCatalog? GetCatalog(ILauncher launcher)
    {
        UbisoftCatalog? localCatalog = null;

        if (launcher.LauncherOptions.LoadLocalCatalogData)
        {
            try
            {
                localCatalog = new UbisoftCatalog(launcher.InstallDir);
            }
            catch { /* ignored */ }
        }

        return localCatalog;
    }

    /// <summary>
    /// Load the Ubisoft game registry entry into a <see cref="UbisoftGame"/> object
    /// </summary>
    private static UbisoftGame? LoadFromRegistry(string gameId)
    {
        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, $@"SOFTWARE\Ubisoft\Launcher\Installs\{gameId}");
        if (regKey is null)
        {
            return null;
        }

        var game = new UbisoftGame()
        {
            Id = gameId,
            InstallDir = PathUtil.Sanitize((string?)regKey.GetValue("InstallDir")) ?? string.Empty,
            Language = (string)regKey.GetValue("Language", string.Empty)!,
        };

        game.Name = Path.GetFileName(game.InstallDir) ?? string.Empty;
        game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;
        game.WorkingDir = game.InstallDir;
        game.LaunchString = $"uplay://launch/{game.Id}";

        return game;
    }

    /// <summary>
    /// Get the executable and game name from the catalog
    /// </summary>
    private static UbisoftGame AddCatalogData(UbisoftGame game, UbisoftCatalog? catalog = null)
    {
        if (catalog?.Catalog
            .Where(p => p.UplayId.ToString() == game.Id)
            .FirstOrDefault(defaultValue: null) is not UbisoftCatalogItem catalogItem)
        {
            return game;
        }

        // get executable, executable path and working dir
        List<UbisoftProductInformation.Executable>? exeList = null;

        exeList ??= catalogItem.GameInfo?.root?.start_game?.offline?.executables;
        exeList ??= catalogItem.GameInfo?.root?.start_game?.online?.executables;

        if (exeList is not null)
        {
            foreach (var exe in exeList
                .Where(p => !string.IsNullOrEmpty(p.path?.relative)))
            {
                game.Executable = PathUtil.Sanitize(Path.Combine(game.InstallDir, exe.path!.relative!))!;
                game.Name = exe.shortcut_name ?? game.Name;

                game.WorkingDir = Path.GetDirectoryName(game.Executable) ?? string.Empty;
                if (exe.working_directory?.register?.StartsWith("HKEY") == false)
                {
                    game.WorkingDir = PathUtil.Sanitize(exe.working_directory.register)!;
                }

                if (!PathUtil.IsExecutable(game.Executable))
                {
                    continue;
                }

                break;
            }
        }

        // get Game name
        string? tmpVal = catalogItem.GameInfo?.root?.name;
        if (!string.IsNullOrEmpty(tmpVal))
        {
            tmpVal = GetLocalizedValue(catalogItem, tmpVal, tmpVal);
        }

        if (tmpVal is "NAME" or "GAMENAME")
        {
            tmpVal = null;
        }

        if (string.IsNullOrEmpty(tmpVal))
        {
            tmpVal = catalogItem.GameInfo?.root?.installer?.game_identifier;
        }

        if (!string.IsNullOrEmpty(tmpVal))
        {
            game.Name = tmpVal;
        }

        // get help URL
        tmpVal = catalogItem.GameInfo?.root?.help_url;
        if (!string.IsNullOrEmpty(tmpVal))
        {
            game.HelpUrl = GetLocalizedValue(catalogItem, tmpVal, tmpVal);
        }

        // get Facebook URL
        tmpVal = catalogItem.GameInfo?.root?.facebook_url;
        if (!string.IsNullOrEmpty(tmpVal))
        {
            game.FacebookUrl = GetLocalizedValue(catalogItem, tmpVal, tmpVal);
        }

        // get homepage URL
        tmpVal = catalogItem.GameInfo?.root?.homepage_url;
        if (!string.IsNullOrEmpty(tmpVal))
        {
            game.HomepageUrl = GetLocalizedValue(catalogItem, tmpVal, tmpVal);
        }

        // get forum URL
        tmpVal = catalogItem.GameInfo?.root?.forum_url;
        if (!string.IsNullOrEmpty(tmpVal))
        {
            game.ForumUrl = GetLocalizedValue(catalogItem, tmpVal, tmpVal);
        }

        return game;
    }

    private static string GetLocalizedValue(UbisoftCatalogItem catalogItem, string name, string defaultValue)
    {
        try
        {
            var value = catalogItem.GameInfo?.localizations?.@default?[name];
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }
        catch { /* ignored */ }

        return defaultValue;
    }
}
