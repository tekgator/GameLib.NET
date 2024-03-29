﻿using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.BattleNet.Model;
using Newtonsoft.Json;
using System.Reflection;

namespace GameLib.Plugin.BattleNet;

public static class BattleNetGameFactory
{
    public static IEnumerable<BattleNetGame> GetGames(ILauncher launcher, CancellationToken cancellationToken = default)
    {
        var catalog = GetCatalog();

        return DeserializeProductInstalls()
            .AsParallel()
            .WithCancellation(cancellationToken)
            .Select(product => BattleNetGameBuiler(launcher, product))
            .Where(game => game is not null)
            .Select(game => AddLauncherId(launcher, game))
            .Select(game => AddCatalogData(launcher, game, catalog))
            .Select(game => AddExecutables(launcher, game))
            .ToList()!;
    }

    /// <summary>
    /// Add launcher ID to Game
    /// </summary>
    private static BattleNetGame AddLauncherId(ILauncher launcher, BattleNetGame game)
    {
        game.LauncherId = launcher.Id;
        return game;
    }

    /// <summary>
    /// Find executables within the install directory
    /// </summary>
    private static BattleNetGame AddExecutables(ILauncher launcher, BattleNetGame game)
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
    /// Load local catalog data
    /// </summary>
    private static BNetGames? GetCatalog()
    {
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? ".", "Resources", "BattleNetGames.json");

        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            var catalog = JsonConvert.DeserializeObject<BNetGames>(json);

            if (catalog is null)
            {
                throw new ApplicationException("Cannot deserialize JSON stream");
            }

            return catalog;
        }
        catch { return null; }
    }

    /// <summary>
    /// Deserialize the BattlNet product.db
    /// </summary>
    private static IEnumerable<ProductInstall> DeserializeProductInstalls()
    {
        var productInstalls = new List<ProductInstall>();

        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Battle.net", "Agent", "product.db");

        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Configuration file not found, probably Battle.net client hasn't been started at least once.", path);
        }

        using var file = File.OpenRead(path);
        productInstalls = ProtoBuf.Serializer.Deserialize<List<ProductInstall>>(file);

        return productInstalls.Where(p => p.Uid is not "agent" and not "battle.net");
    }

    /// <summary>
    /// Convert entry from product.db into a BattleNetGame object
    /// </summary>
    private static BattleNetGame BattleNetGameBuiler(ILauncher launcher, ProductInstall productInstall)
    {
        return new BattleNetGame()
        {
            Id = productInstall.Uid,
            Name = Path.GetFileName(PathUtil.Sanitize(productInstall.Settings.installPath)) ?? string.Empty,
            InstallDir = PathUtil.Sanitize(productInstall.Settings.installPath) ?? string.Empty,
            WorkingDir = PathUtil.Sanitize(productInstall.Settings.installPath) ?? string.Empty,
            InstallDate = PathUtil.GetCreationTime(productInstall.Settings.installPath) ?? DateTime.MinValue,
            LaunchString = $"\"{launcher.Executable}\" --game={productInstall.productCode.ToUpper()}",
            ProductCode = productInstall.productCode ?? string.Empty,
            PlayRegion = productInstall.Settings.playRegion ?? string.Empty,
            SpeechLanguage = productInstall.Settings.selectedSpeechLanguage ?? string.Empty,
            TextLanguage = productInstall.Settings.selectedTextLanguage ?? string.Empty,
            Version = productInstall.cachedProductState.baseProductState.currentVersionStr ?? string.Empty
        };
    }

    /// <summary>
    /// Get the executable and game name from the catalog
    /// </summary>
    private static BattleNetGame AddCatalogData(ILauncher launcher, BattleNetGame game, BNetGames? catalog = null)
    {
        if (catalog?.Games
            .Where(g => g.InternalId == game.Id)
            .FirstOrDefault(defaultValue: null) is not BNetGame catalogItem)
        {
            return game;
        }

        if (!string.IsNullOrEmpty(catalogItem.Name))
        {
            game.Name = catalogItem.Name;
        }

        if (!string.IsNullOrEmpty(catalogItem.ProductId))
        {
            game.ProductCode = catalogItem.ProductId;
            game.LaunchString = $"\"{launcher.Executable}\" --exec=\"launch {game.ProductCode}\"";
        }

        game.Executables = catalogItem.Executables
            .Where(e => !string.IsNullOrEmpty(e))
            .Select(e => Path.Combine(game.InstallDir, e))
            .ToList();

        game.Executable = game.Executables.FirstOrDefault(defaultValue: string.Empty);

        return game;
    }
}
