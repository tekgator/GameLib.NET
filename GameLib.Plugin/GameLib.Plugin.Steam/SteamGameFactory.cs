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
    public static List<SteamGame> GetGames(IEnumerable<SteamLibrary> libraries, SteamCatalogue? catalogue = null)
    {
        List<SteamGame> gameList = new();

        foreach (var library in libraries)
            gameList.AddRange(GetGames(library, catalogue));

        return gameList;
    }

    /// <summary>
    /// Get games installed in for the passed Steam library
    /// </summary>
    public static List<SteamGame> GetGames(SteamLibrary library, SteamCatalogue? catalogue = null)
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

            AddCatalogueData(game, appsPath, catalogue);

            gameList.Add(game);
        }

        return gameList;
    }

    /// <summary>
    /// Get the executable name for the passed Steam App ID from the catalogue
    /// </summary>
    private static void AddCatalogueData(SteamGame game, string appsPath, SteamCatalogue? catalogue)
    {
        if (catalogue?.Catalogue
            .Where(p => p.AppID.ToString() == game.GameId)
            .Where(p => string.IsNullOrEmpty(p.Data?.Common?.OsList) || p.Data.Common.OsList.Contains(_os, StringComparison.OrdinalIgnoreCase))
            .Where(p => p.Data.Config?.Launch is not null)
            .Select(p => p.Data)
            .FirstOrDefault((DeserializedSteamCatalogue.DeserializedData?)null) is not DeserializedSteamCatalogue.DeserializedData catalogueItem)
            return;

        game.Developer = catalogueItem.Extended?.Developer ?? string.Empty;
        game.DeveloperUrl = catalogueItem.Extended?.Developer_URL ?? string.Empty;
        game.Publisher = catalogueItem.Extended?.Publisher ?? string.Empty;
        game.Homepage = catalogueItem.Extended?.Homepage ?? string.Empty;
        game.GameManualUrl = catalogueItem.Extended?.GameManualUrl ?? string.Empty;

        if (catalogueItem.Config?.Launch?
            .Select(p => p.Value)
            .Where(p => string.IsNullOrEmpty(p.Config?.OsList) || p.Config.OsList.Contains(_os, StringComparison.OrdinalIgnoreCase))
            .Where(p => PathUtil.IsExecutable(p.Executable))
            .OrderByDescending(p => p.Config?.OsArch?.Contains(_osArch, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault((DeserializedSteamCatalogue.DeserializedData.DscdLaunch?)null) is not DeserializedSteamCatalogue.DeserializedData.DscdLaunch launcher)
            return;

        game.ExecutablePath = Path.Combine(appsPath, "common", game.InstallDir, PathUtil.Sanitize(launcher.Executable)!);
        game.Executable = Path.GetFileName(game.ExecutablePath);

        if (!string.IsNullOrEmpty(launcher.WorkingDir))
            game.WorkingDir = Path.Combine(appsPath, "common", game.InstallDir, PathUtil.Sanitize(launcher.WorkingDir)!);

        if (string.IsNullOrEmpty(game.WorkingDir))
            game.WorkingDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;

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
