using Gamelib.Util;
using GameLib.Origin.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace GameLib.Origin;

internal static class OriginGameFactory
{
    private static readonly string _os = GetOs();
    private static readonly uint _osArch = GetOsArch();

    public static List<OriginGame> GetGames(bool queryOnlineData)
    {
        var games = new List<OriginGame>();
        var localContentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Origin", "LocalContent");

        if (!Directory.Exists(localContentPath))
            return games;

        Parallel.ForEach(Directory.GetFiles(localContentPath, "*.mfst", SearchOption.AllDirectories), manifest =>
        {
            var manifestText = File.ReadAllText(manifest);
            var valueCollection = HttpUtility.ParseQueryString(HttpUtility.UrlDecode(manifestText));

            var game = new OriginGame()
            {
                GameId = valueCollection["id"] ?? string.Empty,
                InstallDir = PathUtil.Sanitize(valueCollection["dipInstallPath"]) ?? string.Empty,
                Locale = valueCollection["locale"] ?? string.Empty,
            };

            if (string.IsNullOrEmpty(game.GameId) || string.IsNullOrEmpty(game.InstallDir))
                return;

            game.LaunchString = $"origin://launchgame/{game.GameId}";
            game.TotalBytes = long.TryParse(valueCollection["totalbytes"], out long tmpResult) ? tmpResult : 0;

            AddLocalCatalogData(game);
            if (queryOnlineData && (string.IsNullOrEmpty(game.GameName) || string.IsNullOrEmpty(game.ExecutablePath)))
                AddOnlineData(game);

            game.InstallDate = PathUtil.GetCreationTime(game.InstallDir) ?? DateTime.MinValue;
            if (!string.IsNullOrEmpty(game.ExecutablePath))
            {
                game.WorkingDir = Path.GetDirectoryName(game.ExecutablePath) ?? string.Empty;
                game.Executable = Path.GetFileName(game.ExecutablePath);
            }

            games.Add(game);
        });

        return games;
    }

    /// <summary>
    /// Load data from local stored manifest file
    /// In case no GameName is found but a content ID the game name is loaded from the registry
    /// </summary>
    private static void AddLocalCatalogData(OriginGame game)
    {
        var installerXmlPath = Path.Combine(game.InstallDir, "__Installer", "installerdata.xml");
        List<string> contendIds = new();

        if (!AddFromLocalDipManifestData(game, installerXmlPath, contendIds))
            AddFromLocalGameManifestData(game, installerXmlPath, contendIds);

        if (string.IsNullOrEmpty(game.GameName) && contendIds.Count > 0)
            game.GameName = RegistryUtil.GetValue(RegistryHive.LocalMachine, $@"SOFTWARE\Origin Games\{contendIds[0]}", "DisplayName", string.Empty)!;

        if (string.IsNullOrEmpty(game.Locale) && contendIds.Count > 0)
            game.Locale = RegistryUtil.GetValue(RegistryHive.LocalMachine, $@"SOFTWARE\Origin Games\{contendIds[0]}", "Locale", string.Empty)!;
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

        if (string.IsNullOrEmpty(game.GameName))
        {
            var gameTitle = diPManifest.gameTitles?.FirstOrDefault(defaultValue: null);
            game.GameName = gameTitle?.Value ?? game.GameName;
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
                }

                if (!PathUtil.IsExecutable(game.ExecutablePath) && !File.Exists(game.ExecutablePath))
                    game.ExecutablePath = string.Empty;
            }
        }

        return true;
    }

    /// <summary>
    /// Load data from local manifest file in the Game XML schema
    /// Seems to be the case for older Origin games
    /// This schema apparently has no information about the executables of a game
    /// </summary>
    private static bool AddFromLocalGameManifestData(OriginGame game, string installerXmlPath, List<string> contendIds)
    {
        OriginGameManifest? gameManifest = null;
        try
        {
            var ser = new XmlSerializer(typeof(OriginGameManifest));
            gameManifest = ser.Deserialize(XmlReader.Create(installerXmlPath)) as OriginGameManifest;
        }
        catch { /* ignore */ }

        if (gameManifest is null)
            return false;

        if (string.IsNullOrEmpty(game.GameName))
        {
            var gameTitle = gameManifest.metadata?.localeInfo?.FirstOrDefault(defaultValue: null)?.title;
            game.GameName = gameTitle ?? game.GameName;
        }

        if (gameManifest.contentIDs is not null)
            contendIds.AddRange(gameManifest.contentIDs);

        return true;
    }

    /// <summary>
    /// Load data from online manifest file in JSON format
    /// This is the only method to get the executables for older games it seems
    /// </summary>
    private static void AddOnlineData(OriginGame game)
    {
        if (!string.IsNullOrEmpty(game.GameName) && !string.IsNullOrEmpty(game.ExecutablePath))
            return;

        OriginOnlineManifest? manifest = null;
        try
        {
            var manifestJson = GetManifestFromUrl(game.GameId);
            manifest = JsonConvert.DeserializeObject<OriginOnlineManifest>(manifestJson);
            if (manifest is null)
                throw new ApplicationException("Cannot deserialize JSON stream");
        }
        catch { return; }

        if (string.IsNullOrEmpty(game.GameName))
            game.GameName = manifest.LocalizableAttributes?.DisplayName ?? game.GameName;

        if (string.IsNullOrEmpty(game.GameName))
            game.GameName = manifest.ItemName ?? game.GameName;

        if (string.IsNullOrEmpty(game.ExecutablePath) && manifest.Publishing?.SoftwareList?.Software is not null)
        {
            foreach (var item in manifest.Publishing.SoftwareList.Software
                .Where(p => p.SoftwarePlatform is null || p.SoftwarePlatform.Contains(_os))
                .OrderByDescending(p => (p.FulfillmentAttributes?.ProcessorArchitecture ?? string.Empty).Contains(_osArch.ToString())))
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
        }
    }

    /// <summary>
    /// Query manifest JSON string from the Origin API URL
    /// </summary>
    public static string GetManifestFromUrl(string gameId)
    {
        using var client = new HttpClient();

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