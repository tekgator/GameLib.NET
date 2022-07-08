using GameLib.Plugin.Steam.Model;
using System.Collections.ObjectModel;
using ValveKeyValue;

namespace GameLib.Plugin.Steam;

internal class SteamCatalog
{
    private const uint Magic = 0x07_56_44_27;

    private readonly string _catalogPath;
    private SteamUniverse _universe = SteamUniverse.Invalid;
    private List<DeserializedSteamCatalog> _catalog = new();

    public SteamUniverse Universe => _universe;
    public IEnumerable<DeserializedSteamCatalog> Catalog => _catalog;

    public SteamCatalog(string launcherPath)
    {
        _catalogPath = Path.Combine(launcherPath, "appcache", "appinfo.vdf");
        Refresh();
    }

    public void Refresh()
    {
        if (!File.Exists(_catalogPath))
            throw new FileNotFoundException("Configuration file not found, probably Steam client hasn't been started at least once.", _catalogPath);

        _universe = SteamUniverse.Invalid;

        using var stream = File.OpenRead(_catalogPath);
        using var reader = new BinaryReader(stream);

        var magic = reader.ReadUInt32();

        if (magic != Magic)
            throw new InvalidDataException($"Unknown magic header: {magic}");

        _universe = (SteamUniverse)reader.ReadUInt32();

        var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

        List<DeserializedSteamCatalog> deserializedSteamCatalogList = new();
        while (true)
        {
            var appId = reader.ReadUInt32();

            if (appId == 0)
                break;

            DeserializedSteamCatalog item = new()
            {
                AppID = appId,
                Size = reader.ReadUInt32(),
                InfoState = reader.ReadUInt32(),
                LastUpdated = DateTime.UnixEpoch.AddSeconds(reader.ReadUInt32()),
                Token = reader.ReadUInt64(),
                Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                ChangeNumber = reader.ReadUInt32(),
            };

            try
            {
                item.Data = deserializer.Deserialize<DeserializedSteamCatalog.DeserializedData>(stream);
            }
            catch
            {
                continue;
            }

            deserializedSteamCatalogList.Add(item);
        }

        _catalog = deserializedSteamCatalogList;
    }
}
