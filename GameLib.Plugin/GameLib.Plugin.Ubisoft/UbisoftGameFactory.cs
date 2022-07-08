using GameLauncherApi.Ubisoft.Model;
using Gamelib.Util;
using GameLib.Plugin.Ubisoft.Model;
using Microsoft.Win32;

namespace GameLib.Plugin.Ubisoft;

internal class UbisoftGameFactory
{

    public static List<UbisoftGame> GetGames(string? installDir, UbisoftCatalogue? catalogue = null)
    {
        List<UbisoftGame> games = new();

        using var regKey = RegistryUtil.GetKey(RegistryHive.LocalMachine, @"SOFTWARE\Ubisoft\Launcher\Installs", true);

        if (string.IsNullOrEmpty(installDir) || regKey is null)
            return games;

        foreach (var regKeyGameId in regKey.GetSubKeyNames())
        {
            using var regKeyGame = regKey.OpenSubKey(regKeyGameId);
            if (regKeyGame is null)
                continue;

            var game = new UbisoftGame()
            {
                GameId = regKeyGameId,
                InstallDir = PathUtil.Sanitize((string?)regKeyGame.GetValue("InstallDir")) ?? string.Empty,
                Language = (string)regKeyGame.GetValue("Language", string.Empty)!,
            };

            game.GameName = Path.GetFileName(game.InstallDir) ?? string.Empty;
            game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;
            game.WorkingDir = game.InstallDir;
            game.LaunchString = $"uplay://launch/{game.GameId}";

            AddCatalogueData(game, catalogue);

            games.Add(game);
        }

        return games;
    }

    private static void AddCatalogueData(UbisoftGame game, UbisoftCatalogue? catalogue = null)
    {
        if (catalogue?.Catalogue
            .Where(p => p.UplayId.ToString() == game.GameId)
            .FirstOrDefault((UbisoftCatalogueItem?)null) is not UbisoftCatalogueItem catalogueItem)
            return;

        // get executable, executable path and working dir
        List<UbisoftProductInformation.Executable>? exeList = null;

        exeList ??= catalogueItem.GameInfo?.root?.start_game?.offline?.executables;
        exeList ??= catalogueItem.GameInfo?.root?.start_game?.online?.executables;

        if (exeList is not null)
        {
            foreach (var exe in exeList
                .Where(p => !string.IsNullOrEmpty(p.path?.relative)))
            {
                game.ExecutablePath = PathUtil.Sanitize(Path.Combine(game.InstallDir, exe.path!.relative!))!;
                game.GameName = exe.shortcut_name ?? game.GameName;

                game.WorkingDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;
                if (exe.working_directory?.register?.StartsWith("HKEY") == false)
                    game.WorkingDir = PathUtil.Sanitize(exe.working_directory.register)!;

                if (PathUtil.IsExecutable(game.ExecutablePath))
                {
                    game.Executable = Path.GetFileName(game.ExecutablePath) ?? string.Empty;
                    break;
                }

            }
        }

        string? tmpVal = null;

        // get Game name
        tmpVal = catalogueItem.GameInfo?.root?.name;
        if (!string.IsNullOrEmpty(tmpVal))
            tmpVal = GetLocalizedValue(catalogueItem, tmpVal, tmpVal);

        if (tmpVal is "NAME" or "GAMENAME")
            tmpVal = null;

        if (string.IsNullOrEmpty(tmpVal))
            tmpVal = catalogueItem.GameInfo?.root?.installer?.game_identifier;

        if (!string.IsNullOrEmpty(tmpVal))
            game.GameName = tmpVal;

        // get help url
        tmpVal = catalogueItem.GameInfo?.root?.help_url;
        if (!string.IsNullOrEmpty(tmpVal))
            game.HelpUrl = GetLocalizedValue(catalogueItem, tmpVal, tmpVal);

        // get facebook url
        tmpVal = catalogueItem.GameInfo?.root?.facebook_url;
        if (!string.IsNullOrEmpty(tmpVal))
            game.FacebookUrl = GetLocalizedValue(catalogueItem, tmpVal, tmpVal);

        // get homepage url
        tmpVal = catalogueItem.GameInfo?.root?.homepage_url;
        if (!string.IsNullOrEmpty(tmpVal))
            game.HomepageUrl = GetLocalizedValue(catalogueItem, tmpVal, tmpVal);

        // get forum url
        tmpVal = catalogueItem.GameInfo?.root?.forum_url;
        if (!string.IsNullOrEmpty(tmpVal))
            game.ForumUrl = GetLocalizedValue(catalogueItem, tmpVal, tmpVal);
    }

    private static string GetLocalizedValue(UbisoftCatalogueItem catalogueItem, string name, string defaultValue)
    {
        try
        {
            var value = catalogueItem?.GameInfo?.localizations?.@default?[name];
            if (!string.IsNullOrEmpty(value))
                return value;
        }
        catch { }

        return defaultValue;
    }

}
