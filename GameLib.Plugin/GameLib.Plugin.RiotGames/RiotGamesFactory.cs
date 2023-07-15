﻿using GameLib.Core;
using GameLib.Plugin.RiotGames.Model;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace GameLib.Plugin.RiotGames
{
    internal static class RiotGamesFactory
    {
        private static readonly List<string> Processes = new List<string> { "RiotClientServices", "RiotClientUx", "RiotClientUxRender",
            "VALORANT-Win64-Shipping", "LeagueClient", "LeagueClientUx", "League of Legends", "LoR" };
        public static async Task<IEnumerable<IGame>> GetGames(ILauncher launcher, CancellationToken cancellationToken = default)
        {
            var games = new List<IGame>();
            var basePath = @"C:\\ProgramData\\Riot Games\\Metadata";
            Regex regex = new Regex("^(.*\\.)?live$");
            var directories = Directory.GetDirectories(basePath);
            foreach (var d in directories)
            {
                if (!regex.IsMatch(d))
                {
                    continue;
                }
                string[] splittedPath = new DirectoryInfo(d).Name.Split(".live");
                
                string path = Path.Combine(basePath, d);
                if (!Directory.Exists(path))
                {
                    continue;
                }
                path = Path.Join(path, @$"{splittedPath[0]}.live.product_settings.yaml");
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
                string fullPath;
                string shortcut;
                if (!yamlData.ContainsKey("product_install_full_path") || !yamlData.ContainsKey("shortcut_name"))
                {
                    continue;
                }
                fullPath = yamlData["product_install_full_path"];
                shortcut = yamlData["shortcut_name"];
                
                games.Add(new Game
                {
                    Id = yamlData.GetHashCode().ToString(),
                    LauncherId = launcher.Id,
                    Name = shortcut.Split(".lnk")[0],
                    InstallDir = fullPath,
                    Executables = Processes,
                    WorkingDir = fullPath,
                    LaunchString = $"--launch-product={splittedPath[0]} --launch-patchline=live"
                });

            }
            return games;


        }
    }
}