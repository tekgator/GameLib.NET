using Gamelib.Util;
using GameLib.Plugin.Epic.Model;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace GameLib.Plugin.Epic;

internal static class EpicGameFactory
{
    public static List<EpicGame> GetGames()
    {
        List<EpicGame> gameList = new();

        var metaDataDir = GetMetadataDir();
        if (string.IsNullOrEmpty(metaDataDir))
            return gameList;

        foreach (var manifestFile in Directory.GetFiles(metaDataDir, "*.item"))
        {
            DeserializedEpicGame? deserializedEpicGame;
            try
            {
                var manifestJson = File.ReadAllText(manifestFile);
                deserializedEpicGame = JsonConvert.DeserializeObject<DeserializedEpicGame>(manifestJson);
                if (deserializedEpicGame is null)
                    throw new ApplicationException("Cannot deserialize JSON stream");
            }
            catch { continue; }

            var game = deserializedEpicGame.EpicGameBuilder();

            game.ExecutablePath = Path.Combine(PathUtil.Sanitize(game.InstallDir)!, game.Executable);
            game.LaunchString = $"com.epicgames.launcher://apps/{game.GameId}?action=launch&silent=true";
            game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;

            gameList.Add(game);
        }

        return gameList;
    }

    private static string? GetMetadataDir()
    {
        string? metadataDir = null;

        metadataDir ??= RegistryUtil.GetValue(RegistryHive.CurrentUser, @"Software\Epic Games\EOS", "ModSdkMetadataDir");
        metadataDir ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"Software\Epic Games\EpicGamesLauncher", "AppDataPath");
        metadataDir ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic", "EpicGamesLauncher", "Data");

        metadataDir = PathUtil.Sanitize(metadataDir);
        if (string.IsNullOrEmpty(metadataDir))
            return null;

        if (!metadataDir.EndsWith("Manifests", StringComparison.OrdinalIgnoreCase))
            metadataDir = Path.Combine(metadataDir, "Manifests");

        if (!Directory.Exists(metadataDir))
            return null;

        return metadataDir;
    }
}
