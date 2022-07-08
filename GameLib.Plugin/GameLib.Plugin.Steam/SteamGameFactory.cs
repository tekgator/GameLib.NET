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
    public static List<SteamGame> GetGames(IEnumerable<SteamLibrary> libraries, SteamCatalog? catalog = null)
    {
        List<SteamGame> gameList = new();

        foreach (var library in libraries)
            gameList.AddRange(GetGames(library, catalog));

        return gameList;
    }

    /// <summary>
    /// Get games installed in for the passed Steam library
    /// </summary>
    public static List<SteamGame> GetGames(SteamLibrary library, SteamCatalog? catalog = null)
    {
        List<SteamGame> gameList = new();

        var appsPath = Path.Combine(library.Path, "steamapps");
        if (!Directory.Exists(appsPath))
            return gameList;

        var serializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

        foreach (var file in Directory.GetFiles(appsPath, "*.acf"))
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
            game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;

            AddCatalogData(game, appsPath, catalog);

            gameList.Add(game);
        }

        return gameList;
    }

    /// <summary>
    /// Get the executable name for the passed Steam App ID from the catalog
    /// </summary>
    private static void AddCatalogData(SteamGame game, string appsPath, SteamCatalog? catalog)
    {
        if (catalog?.Catalog
            .Where(p =>
                p.AppID.ToString() == game.GameId &&
                (string.IsNullOrEmpty(p.Data.Common?.OsList) || p.Data.Common.OsList.Contains(_os, StringComparison.OrdinalIgnoreCase)) &&
                p.Data.Config?.Launch is not null)
            .Select(p => p.Data)
            .FirstOrDefault((DeserializedSteamCatalog.DeserializedData?)null) is not { } catalogItem)
        {
            return;
        }

        game.Developer = catalogItem.Extended?.Developer ?? string.Empty;
        game.DeveloperUrl = catalogItem.Extended?.Developer_URL ?? string.Empty;
        game.Publisher = catalogItem.Extended?.Publisher ?? string.Empty;
        game.Homepage = catalogItem.Extended?.Homepage ?? string.Empty;
        game.GameManualUrl = catalogItem.Extended?.GameManualUrl ?? string.Empty;

        if (catalogItem.Config?.Launch?
            .Select(p => p.Value)
            .Where(p =>
                (string.IsNullOrEmpty(p.Config?.OsList) || p.Config.OsList.Contains(_os, StringComparison.OrdinalIgnoreCase)) &&
                PathUtil.IsExecutable(p.Executable))
            .OrderByDescending(p => p.Config?.OsArch?.Contains(_osArch, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault((DeserializedSteamCatalog.DeserializedData.DscdLaunch?)null) is not { } launcher)
        {
            return;
        }

        game.ExecutablePath = Path.Combine(appsPath, "common", game.InstallDir, PathUtil.Sanitize(launcher.Executable)!);
        game.Executable = Path.GetFileName(game.ExecutablePath);

        if (!string.IsNullOrEmpty(launcher.WorkingDir))
            game.WorkingDir = Path.Combine(appsPath, "common", game.InstallDir, PathUtil.Sanitize(launcher.WorkingDir)!);

        if (string.IsNullOrEmpty(game.WorkingDir))
            game.WorkingDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;
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
