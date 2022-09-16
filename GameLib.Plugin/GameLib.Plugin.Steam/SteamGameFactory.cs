using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Steam.Model;
using System.Runtime.InteropServices;
using ValveKeyValue;

namespace GameLib.Plugin.Steam;

internal static class SteamGameFactory
{
    private static readonly string Os = GetOs();
    private static readonly string OsArch = GetOsArch();

    /// <summary>
    /// Get games installed in for the passed Steam libraries
    /// </summary>
    public static IEnumerable<SteamGame> GetGames(ILauncher launcher, IEnumerable<SteamLibrary> libraries, CancellationToken cancellationToken = default) =>
        libraries.SelectMany(lib => GetGames(launcher, lib, cancellationToken)).ToList();

    /// <summary>
    /// Get games installed in for the passed Steam library
    /// </summary>
    public static IEnumerable<SteamGame> GetGames(ILauncher launcher, SteamLibrary library, CancellationToken cancellationToken = default)
    {
        var appsPath = Path.Combine(library.Path, "steamapps");
        if (!Directory.Exists(appsPath))
        {
            return Enumerable.Empty<SteamGame>();
        }

        SteamCatalog? localCatalog = GetCatalog(launcher);

        return Directory.GetFiles(appsPath, "*.acf")
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(manifestFile => DeserializeManifest(library.Path, manifestFile))
            .Where(game => game is not null)
            .Select(game => AddLauncherId(launcher, game!))
            .Select(game => AddExecutables(launcher, game!))
            .Select(game => AddCatalogData(game!, appsPath, localCatalog))
            .ToList();
    }

    /// <summary>
    /// Add launcher ID to Game
    /// </summary>
    private static SteamGame AddLauncherId(ILauncher launcher, SteamGame game)
    {
        game.LauncherId = launcher.Id;
        return game;
    }

    /// <summary>
    /// Find executables within the install directory
    /// </summary>
    private static SteamGame AddExecutables(ILauncher launcher, SteamGame game)
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
    /// Load steam local catalog data
    /// </summary>
    private static SteamCatalog? GetCatalog(ILauncher launcher)
    {
        SteamCatalog? localCatalog = null;

        if (launcher.LauncherOptions.LoadLocalCatalogData)
        {
            try
            {
                localCatalog = new SteamCatalog(launcher.InstallDir);
            }
            catch { /* ignored */ }
        }

        return localCatalog;
    }

    /// <summary>
    /// Deserialize the Steam Game manifest file into a <see cref="SteamGame"/> object
    /// </summary>
    private static SteamGame? DeserializeManifest(string libraryPath, string manifestFile)
    {
        SteamGame game;
        try
        {
            using var stream = File.OpenRead(manifestFile);
            var serializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);
            var deserializedSteamGame = serializer.Deserialize<DeserializedSteamGame>(stream);

            game = new()
            {
                Id = deserializedSteamGame.AppId.ToString(),
                Name = deserializedSteamGame.Name ?? string.Empty,
                InstallDir = deserializedSteamGame.InstallDir ?? string.Empty,
                Universe = (SteamUniverse)deserializedSteamGame.Universe,
                LastUpdated = DateTimeOffset.FromUnixTimeSeconds(deserializedSteamGame.LastUpdated).LocalDateTime,
                SizeOnDisk = deserializedSteamGame.SizeOnDisk,
                LastOwner = deserializedSteamGame.LastOwner,
                AutoUpdateBehavior = deserializedSteamGame.AutoUpdateBehavior,
                AllowOtherDownloadsWhileRunning = deserializedSteamGame.AllowOtherDownloadsWhileRunning,
                ScheduledAutoUpdate = DateTimeOffset.FromUnixTimeSeconds(deserializedSteamGame.ScheduledAutoUpdate).LocalDateTime,
            };
        }
        catch
        {
            return null;
        }

        game.LaunchString = $"steam://rungameid/{game.Id}";
        game.InstallDir = Path.Combine(libraryPath, "steamapps", "common", PathUtil.Sanitize(game.InstallDir) ?? string.Empty);
        game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;

        return game;
    }

    /// <summary>
    /// Get the executable name for the passed Steam App ID from the catalog
    /// </summary>
    private static SteamGame AddCatalogData(SteamGame game, string appsPath, SteamCatalog? catalog)
    {
        if (catalog?.Catalog
            .Where(p =>
                p.AppID.ToString() == game.Id &&
                (string.IsNullOrEmpty(p.Data.Common?.OsList) || p.Data.Common.OsList.Contains(Os, StringComparison.OrdinalIgnoreCase)) &&
                p.Data.Config?.Launch is not null)
            .Select(p => p.Data)
            .FirstOrDefault(defaultValue: null) is not DeserializedSteamCatalog.DeserializedData catalogItem)
        {
            return game;
        }

        game.Developer = catalogItem.Extended?.Developer ?? string.Empty;
        game.DeveloperUrl = catalogItem.Extended?.Developer_URL ?? string.Empty;
        game.Publisher = catalogItem.Extended?.Publisher ?? string.Empty;
        game.Homepage = catalogItem.Extended?.Homepage ?? string.Empty;
        game.GameManualUrl = catalogItem.Extended?.GameManualUrl ?? string.Empty;

        if (catalogItem.Config?.Launch?
            .Select(p => p.Value)
            .Where(p =>
                (string.IsNullOrEmpty(p.Config?.OsList) || p.Config.OsList.Contains(Os, StringComparison.OrdinalIgnoreCase)) &&
                PathUtil.IsExecutable(p.Executable))
            .OrderByDescending(p => p.Config?.OsArch?.Contains(OsArch, StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(defaultValue: null) is not DeserializedSteamCatalog.DeserializedData.DscdLaunch launcher)
        {
            return game;
        }

        game.Executable = Path.Combine(appsPath, "common", game.InstallDir, PathUtil.Sanitize(launcher.Executable)!);

        if (!string.IsNullOrEmpty(launcher.WorkingDir))
        {
            game.WorkingDir = Path.Combine(appsPath, "common", game.InstallDir, PathUtil.Sanitize(launcher.WorkingDir)!);
        }

        if (string.IsNullOrEmpty(game.WorkingDir))
        {
            game.WorkingDir = Path.GetDirectoryName(game.Executable) ?? string.Empty;
        }

        return game;
    }

    /// <summary>
    /// Return the OS as a valid Steam string
    /// </summary>
    private static string GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "windows";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "macos";
        }

        return string.Empty;
    }

    /// <summary>
    /// Returns the OS architecture as an number value only
    /// </summary>
    private static string GetOsArch() => (RuntimeInformation.OSArchitecture == Architecture.X64 ? 64 : 32).ToString();
}
