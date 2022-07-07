using GameLib.Plugin.Steam.Model;
using System.Collections.ObjectModel;
using ValveKeyValue;

namespace GameLib.Plugin.Steam;

internal class SteamCatalogue
{
    private const uint Magic = 0x07_56_44_27;

    private readonly string _cataloguePath;
    private SteamUniverse _universe = SteamUniverse.Invalid;
    private List<DeserializedSteamCatalogue> _catalogue = new();

    public SteamUniverse Universe => _universe;
    public List<DeserializedSteamCatalogue> Catalogue => _catalogue;

    public SteamCatalogue(string launcherPath)
    {
        _cataloguePath = Path.Combine(launcherPath, "appcache", "appinfo.vdf");
        Refresh();
    }

    public void Refresh()
    {
        _universe = SteamUniverse.Invalid;

        using var stream = File.OpenRead(_cataloguePath);
        using var reader = new BinaryReader(stream);

        var magic = reader.ReadUInt32();

        if (magic != Magic)
            throw new InvalidDataException($"Unknown magic header: {magic}");

        _universe = (SteamUniverse)reader.ReadUInt32();

        var deserializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Binary);

        List<DeserializedSteamCatalogue> deserializedSteamCatalogueList = new();
        do
        {
            var appId = reader.ReadUInt32();

            if (appId == 0)
                break;

            DeserializedSteamCatalogue item = new()
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
                item.Data = deserializer.Deserialize<DeserializedSteamCatalogue.DeserializedData>(stream);
            }
            catch
            {
                continue;
            }

            deserializedSteamCatalogueList.Add(item);
        } while (true);

        _catalogue = deserializedSteamCatalogueList;
    }
}
