using Gamelib.Util;
using GameLib.Plugin.Steam.Model;
using System.Runtime.InteropServices;
using ValveKeyValue;

namespace GameLib.Plugin.Steam;

internal static class SteamGameFactory
{
    private static readonly string _os = GetOs();
    private static readonly string _osArch = GetOsArch();

    /// <summary>
    /// Get games installed in for the passed Steam libraries
    /// </summary>
    public static List<SteamGame> GetGames(IEnumerable<SteamLibrary> libraries, SteamCatalogue? steamCatalogue = null)
    {
        List<SteamGame> gameList = new();

        foreach (var library in libraries)
            gameList.AddRange(GetGames(library, steamCatalogue));

        return gameList;
    }

    /// <summary>
    /// Get games installed in for the passed Steam library
    /// </summary>
    public static List<SteamGame> GetGames(SteamLibrary library, SteamCatalogue? steamCatalogue = null)
    {
        List<SteamGame> gameList = new();

        var steamAppsPath = Path.Combine(library.Path, "steamapps");
        if (!Directory.Exists(steamAppsPath))
            return gameList;

        var serializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

        foreach (var file in Directory.GetFiles(steamAppsPath, "*.acf"))
        {
            using var stream = File.OpenRead(file);

            SteamGame game;
            try
            {
                game = serializer.Deserialize<DeserializedSteamGame>(stream).SteamGameBuilder();
            }
            catch
            {
                continue;
            }

            game.LaunchString = $"steam://rungameid/{game.GameId}";
            game.InstallDir = Path.Combine(library.Path, "steamapps", "common", PathUtil.Sanitize(game.InstallDir) ?? string.Empty);
            game.InstallDate = Directory.GetCreationTime(game.InstallDir);

            AddCatalogueData(game, steamAppsPath, steamCatalogue);

            gameList.Add(game);
        }

        return gameList;
    }

    /// <summary>
    /// Get the executable name for the passed Steam App ID from the catalogue
    /// </summary>
    private static void AddCatalogueData(SteamGame game, string steamAppsPath, SteamCatalogue? steamCatalogue)
    {
        if (steamCatalogue is null)
            return;

        var catalogueItem = steamCatalogue.Catalogue
            .Where(p => p.AppID.ToString() == game.GameId)
            .Where(p => string.IsNullOrEmpty(p.Data?.Common?.OsList) || p.Data.Common.OsList.Contains(_os, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.Data.Config?.Launch is not null)
            .Select(p => p.Data)
            .FirstOrDefault((DeserializedSteamCatalogue.DeserializedData?)null);

        if (catalogueItem is null)
            return;

        var launcher = catalogueItem.Config?.Launch?
            .Select(p => p.Value)
            .Where(p => string.IsNullOrEmpty(p.Config?.OsList) || p.Config.OsList.Contains(_os, StringComparison.OrdinalIgnoreCase))
            .Where(p => PathUtil.IsExecutable(p.Executable))
            .OrderByDescending(p => p.Config?.OsArch?.Contains(_osArch, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault((DeserializedSteamCatalogue.DeserializedData.DscdLaunch?)null);

        if (launcher is null)
            return;

        game.ExecutablePath = Path.Combine(steamAppsPath, "common", game.InstallDir, PathUtil.Sanitize(launcher.Executable)!);
        game.Executable = Path.GetFileName(game.ExecutablePath);

        if (!string.IsNullOrEmpty(launcher.WorkingDir))
            game.WorkingDir = Path.Combine(steamAppsPath, "common", game.InstallDir, PathUtil.Sanitize(launcher.WorkingDir)!);

        if (string.IsNullOrEmpty(game.WorkingDir))
            game.WorkingDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;

        game.Developer = catalogueItem.Extended?.Developer ?? string.Empty;
        game.DeveloperUrl = catalogueItem.Extended?.Developer_URL ?? string.Empty;
        game.Publisher = catalogueItem.Extended?.Publisher ?? string.Empty;
        game.Homepage = catalogueItem.Extended?.Homepage ?? string.Empty;
        game.GameManualUrl = catalogueItem.Extended?.GameManualUrl ?? string.Empty;

        return;
    }

    /// <summary>
    /// Return the OS as a valid Steam string
    /// </summary>
    private static string GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "windows";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "linux";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "macos";

        return string.Empty;
    }

    /// <summary>
    /// Returns the OS architecture as an number value only
    /// </summary>
    private static string GetOsArch() => (RuntimeInformation.OSArchitecture == Architecture.X64 ? 64 : 32).ToString();
}
