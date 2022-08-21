using Gamelib.Core.Util;
using GameLib.Plugin.Origin.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace GameLib.Plugin.Origin;

internal static class OriginGameFactory
{
    private static readonly string Os = GetOs();
    private static readonly uint OsArch = GetOsArch();

    /// <summary>
    /// Get games installed for the Origin launcher
    /// </summary>
    public static IEnumerable<OriginGame> GetGames(Guid launcherId, bool queryOnlineData, TimeSpan? queryTimeout = null, CancellationToken cancellationToken = default)
    {
        var localContentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Origin", "LocalContent");

        if (!Directory.Exists(localContentPath))
            return Enumerable.Empty<OriginGame>();

        return Directory.GetFiles(localContentPath, "*.mfst", SearchOption.AllDirectories)
            //.AsParallel()
            //.WithCancellation(cancellationToken)
            .Select(manifestFile => DeserializeManifest(manifestFile))
            .Where(game => game is not null)
            .Select(game => { game!.LauncherId = launcherId; return game; })
            .Select(game => AddLocalCatalogData(game!))
            .Select(game => queryOnlineData ? AddOnlineData(game, queryTimeout) : game)
            .ToList();
    }

    /// <summary>
    /// Deserialize the Origin Game manifest file into a <see cref="OriginGame"/> object
    /// </summary>
    private static OriginGame? DeserializeManifest(string manifestFile)
    {
        var manifestText = File.ReadAllText(manifestFile);
        var valueCollection = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(manifestText));

        var game = new OriginGame()
        {
            Id = valueCollection["id"] ?? string.Empty,
            InstallDir = PathUtil.Sanitize(valueCollection["dipInstallPath"]) ?? string.Empty,
            Locale = valueCollection["locale"] ?? string.Empty,
        };

        if (string.IsNullOrEmpty(game.Id) || string.IsNullOrEmpty(game.InstallDir))
            return null;

        game.LaunchString = $"origin://launchgame/{game.Id}";
        game.TotalBytes = long.TryParse(valueCollection["totalbytes"], out long tmpResult) ? tmpResult : 0;
        game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;

        return game;
    }

    /// <summary>
    /// Load data from local stored manifest file
    /// In case no GameName is found but a content ID the game name is loaded from the registry
    /// </summary>
    private static OriginGame AddLocalCatalogData(OriginGame game)
    {
        var installerXmlPath = Path.Combine(game.InstallDir, "__Installer", "installerdata.xml");
        List<string> contendIds = new();

        if (!AddFromLocalDipManifestData(game, installerXmlPath, contendIds))
            AddFromLocalGameManifestData(game, installerXmlPath, contendIds);

        if (string.IsNullOrEmpty(game.Name) && contendIds.Count > 0)
            game.Name = RegistryUtil.GetValue(RegistryHive.LocalMachine, $@"SOFTWARE\Origin Games\{contendIds[0]}", "DisplayName", string.Empty)!;

        if (string.IsNullOrEmpty(game.Locale) && contendIds.Count > 0)
            game.Locale = RegistryUtil.GetValue(RegistryHive.LocalMachine, $@"SOFTWARE\Origin Games\{contendIds[0]}", "Locale", string.Empty)!;

        if (!string.IsNullOrEmpty(game.ExecutablePath))
        {
            game.WorkingDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;
            game.Executable = Path.GetFileName(game.ExecutablePath);
        }

        return game;
    }

    /// <summary>
    /// Load data from local manifest file in the DIP XML schema
    /// Seems to be the case for newer Origin games
    /// </summary>
    private static bool AddFromLocalDipManifestData(OriginGame game, string installerXmlPath, List<string> contendIds)
    {
        OriginDiPManifest? diPManifest = null;
        try
        {
            var ser = new XmlSerializer(typeof(OriginDiPManifest));
            diPManifest = ser.Deserialize(XmlReader.Create(installerXmlPath)) as OriginDiPManifest;
        }
        catch { /* ignore */ }

        if (diPManifest is null)
            return false;

        if (string.IsNullOrEmpty(game.Name))
        {
            var gameTitle = diPManifest.gameTitles?.FirstOrDefault(defaultValue: null);
            game.Name = gameTitle?.Value ?? game.Name;
        }

        if (diPManifest.contentIDs is not null)
            contendIds.AddRange(diPManifest.contentIDs);

        if (string.IsNullOrEmpty(game.ExecutablePath))
        {
            var filePath = diPManifest.runtime?.FirstOrDefault(defaultValue: null)?.filePath;
            if (!string.IsNullOrEmpty(filePath))
            {
                game.ExecutablePath = filePath;
                if (filePath.StartsWith('[') && filePath.Contains(']'))
                {
                    game.ExecutablePath = PathUtil.Sanitize(Path.Combine(game.InstallDir, filePath[(filePath.LastIndexOf(']') + 1)..]))!;
                    game.WorkingDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;
                    game.Executable = Path.GetFileName(game.ExecutablePath);
                }

                if (!PathUtil.IsExecutable(game.ExecutablePath) && !File.Exists(game.ExecutablePath))
                {
                    game.ExecutablePath = string.Empty;
                    game.WorkingDir = string.Empty;
                    game.Executable = string.Empty;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Load data from local manifest file in the Game XML schema
    /// Seems to be the case for older Origin games
    /// This schema apparently has no information about the executables of a game
    /// </summary>
    private static void AddFromLocalGameManifestData(OriginGame game, string installerXmlPath, List<string> contendIds)
    {
        OriginGameManifest? gameManifest = null;
        try
        {
            var ser = new XmlSerializer(typeof(OriginGameManifest));
            gameManifest = ser.Deserialize(XmlReader.Create(installerXmlPath)) as OriginGameManifest;
        }
        catch { /* ignore */ }

        if (gameManifest is null)
            return;

        if (string.IsNullOrEmpty(game.Name))
        {
            var gameTitle = gameManifest.metadata?.localeInfo?.FirstOrDefault(defaultValue: null)?.title;
            game.Name = gameTitle ?? game.Name;
        }

        if (gameManifest.contentIDs is not null)
            contendIds.AddRange(gameManifest.contentIDs);
    }

    /// <summary>
    /// Load data from online manifest file in JSON format
    /// This is the only method to get the executables for older games it seems
    /// </summary>
    private static OriginGame AddOnlineData(OriginGame game, TimeSpan? queryTimeout = null)
    {
        if (!string.IsNullOrEmpty(game.Name) && !string.IsNullOrEmpty(game.ExecutablePath))
            return game;

        OriginOnlineManifest? manifest;
        try
        {
            var manifestJson = GetManifestFromUrl(game.Id, queryTimeout);
            manifest = JsonConvert.DeserializeObject<OriginOnlineManifest>(manifestJson);
            if (manifest is null)
                throw new ApplicationException("Cannot deserialize JSON stream");
        }
        catch { return game; }

        if (string.IsNullOrEmpty(game.Name))
            game.Name = manifest.LocalizableAttributes?.DisplayName ?? game.Name;

        if (string.IsNullOrEmpty(game.Name))
            game.Name = manifest.ItemName ?? game.Name;

        if (string.IsNullOrEmpty(game.ExecutablePath) && manifest.Publishing?.SoftwareList?.Software is not null)
        {
            foreach (var item in manifest.Publishing.SoftwareList.Software
                .Where(p => p.SoftwarePlatform is null || p.SoftwarePlatform.Contains(Os))
                .OrderByDescending(p => (p.FulfillmentAttributes?.ProcessorArchitecture ?? string.Empty).Contains(OsArch.ToString())))
            {
                var filePath = item.FulfillmentAttributes?.ExecutePathOverride ?? game.ExecutablePath;
                if (!string.IsNullOrEmpty(filePath))
                {
                    game.ExecutablePath = filePath;
                    if (filePath.StartsWith('[') && filePath.Contains(']'))
                    {
                        game.ExecutablePath = Path.Combine(game.InstallDir, PathUtil.Sanitize(filePath[(filePath.LastIndexOf(']') + 1)..])!);
                    }

                    if (PathUtil.IsExecutable(game.ExecutablePath) && File.Exists(game.ExecutablePath))
                        break;

                    game.ExecutablePath = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(game.ExecutablePath))
            {
                game.WorkingDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;
                game.Executable = Path.GetFileName(game.ExecutablePath);
            }
        }

        return game;
    }

    /// <summary>
    /// Query manifest JSON string from the Origin API URL
    /// </summary>
    public static string GetManifestFromUrl(string gameId, TimeSpan? queryTimeout = null)
    {
        using var client = new HttpClient();
        if (queryTimeout is not null)
            client.Timeout = queryTimeout.Value;

        var url = $"https://api1.origin.com/ecommerce2/public/{gameId}/en_US";
        using var webRequest = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = client.Send(webRequest);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException("Response is not OK", null, response.StatusCode);

        using var reader = new StreamReader(response.Content.ReadAsStream());

        return reader.ReadToEnd();
    }

    /// <summary>
    /// Return the OS as a valid Origin string (as per manifest)
    /// </summary>
    private static string GetOs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "PCWIN";

        // TODO: haven't seen a Origin Linux game yet, this line might need to be adjusted
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "LINUX";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "MAC";

        return string.Empty;
    }

    /// <summary>
    /// Returns the OS architecture as an integer
    /// </summary>
    private static uint GetOsArch() => (uint)(RuntimeInformation.OSArchitecture == Architecture.X64 ? 64 : 32);
}