using GameLib.Core;
using GameLib.Plugin.RiotGames.Model;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace GameLib.Plugin.RiotGames;

internal static class RiotGamesFactory
{
    private static readonly List<string> Processes = new()
    {
        "RiotClientServices",
        "RiotClientUx",
        "RiotClientUxRender",
        "VALORANT-Win64-Shipping",
        "LeagueClient",
        "LeagueClientUx",
        "League of Legends",
        "LoR"
    };
            
    public static async Task<IEnumerable<IGame>> GetGames(ILauncher launcher, CancellationToken cancellationToken = default)
    {
        var games = new List<IGame>();
        var basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Riot Games\\Metadata");
        var regex = new Regex("^(.*\\.)?live$");

        foreach (var d in Directory.GetDirectories(basePath))
        {
            if (!regex.IsMatch(d))
            {
                continue;
            }
            var splitPath = new DirectoryInfo(d).Name.Split(".live");
                
            var path = Path.Combine(basePath, d);
            if (!Directory.Exists(path))
            {
                continue;
            }
            path = Path.Join(path, @$"{splitPath[0]}.live.product_settings.yaml");
            if (!File.Exists(path))
            {
                continue;
            }
            var streamData = new StringReader(await File.ReadAllTextAsync(path, cancellationToken));
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.LowerCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var yamlData = deserializer.Deserialize<dynamic>(streamData);
            if (!yamlData.ContainsKey("product_install_full_path") || !yamlData.ContainsKey("shortcut_name"))
            {
                continue;
            }
                
            string fullPath = yamlData["product_install_full_path"];
            string shortcut = yamlData["shortcut_name"];
                
            games.Add(new Game
            {
                Id = yamlData.GetHashCode().ToString(),
                LauncherId = launcher.Id,
                Name = shortcut.Split(".lnk")[0],
                InstallDir = fullPath,
                Executables = Processes,
                WorkingDir = fullPath,
                LaunchString = $"--launch-product={splitPath[0]} --launch-patchline=live"
            });

        }
            
        return games;
    }
}