using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Epic.Model;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace GameLib.Plugin.Epic;

internal static class EpicGameFactory
{
    public static IEnumerable<EpicGame> GetGames(ILauncher launcher, CancellationToken cancellationToken = default)
    {
        var metaDataDir = GetMetadataDir();
        if (string.IsNullOrEmpty(metaDataDir))
        {
            return Enumerable.Empty<EpicGame>();
        }

        return Directory.GetFiles(metaDataDir, "*.item")
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(DeserializeManifest)
            .Where(game => game is not null)
            .Select(game => AddLauncherId(launcher, game!))
            .Select(game => AddExecutables(launcher, game!))
            .ToList()!;
    }

    /// <summary>
    /// Add launcher ID to Game
    /// </summary>
    private static EpicGame AddLauncherId(ILauncher launcher, EpicGame game)
    {
        game.LauncherId = launcher.Id;
        return game;
    }

    /// <summary>
    /// Find executables within the install directory
    /// </summary>
    private static EpicGame AddExecutables(ILauncher launcher, EpicGame game)
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
    /// Get the meta data directory from registry; if not found try to locate in Common Application data
    /// </summary>
    private static string? GetMetadataDir()
    {
        string? metadataDir = null;

        metadataDir ??= RegistryUtil.GetValue(RegistryHive.CurrentUser, @"Software\Epic Games\EOS", "ModSdkMetadataDir");
        metadataDir ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"Software\Epic Games\EpicGamesLauncher", "AppDataPath");
        metadataDir ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic", "EpicGamesLauncher", "Data");

        metadataDir = PathUtil.Sanitize(metadataDir);
        if (string.IsNullOrEmpty(metadataDir))
        {
            return null;
        }

        if (!metadataDir.EndsWith("Manifests", StringComparison.OrdinalIgnoreCase))
        {
            metadataDir = Path.Combine(metadataDir, "Manifests");
        }

        if (!Directory.Exists(metadataDir))
        {
            return null;
        }

        return metadataDir;
    }

    /// <summary>
    /// Deserialize the Epic game manifest file into a <see cref="EpicGame"/> object
    /// </summary>
    private static EpicGame? DeserializeManifest(string manifestFile)
    {
        DeserializedEpicGame? deserializedEpicGame;
        try
        {
            var manifestJson = File.ReadAllText(manifestFile);
            deserializedEpicGame = JsonConvert.DeserializeObject<DeserializedEpicGame>(manifestJson);
            if (deserializedEpicGame is null)
            {
                throw new ApplicationException("Cannot deserialize JSON stream");
            }
        }
        catch { return null; }

        var game = new EpicGame()
        {
            Id = deserializedEpicGame.AppName,
            Name = deserializedEpicGame.DisplayName,
            InstallDir = PathUtil.Sanitize(deserializedEpicGame.InstallLocation) ?? string.Empty,
            WorkingDir = deserializedEpicGame.InstallLocation,
            InstallSize = deserializedEpicGame.InstallSize,
            Version = deserializedEpicGame.AppVersionString,
        };

        if (PathUtil.IsExecutable(deserializedEpicGame.MainWindowProcessName))
        {
            game.Executable = Path.Combine(game.InstallDir, deserializedEpicGame.MainWindowProcessName);
            if (PathUtil.IsExecutable(deserializedEpicGame.MainWindowProcessName))
            {
                game.Executables = new List<string>() { Path.Combine(game.InstallDir, deserializedEpicGame.LaunchExecutable) };
            }
        }
        else if (PathUtil.IsExecutable(deserializedEpicGame.LaunchExecutable))
        {
            game.Executable = Path.Combine(game.InstallDir, deserializedEpicGame.LaunchExecutable);
        }

        game.LaunchString = $"com.epicgames.launcher://apps/{game.Id}?action=launch&silent=true";
        game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;

        return game;
    }
}
