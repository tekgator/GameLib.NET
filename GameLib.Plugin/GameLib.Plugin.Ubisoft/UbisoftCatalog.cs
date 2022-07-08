using GameLib.Plugin.Ubisoft.Model;
using YamlDotNet.Serialization;

namespace GameLib.Plugin.Ubisoft;

internal class UbisoftCatalog
{
    private readonly string _catalogPath;
    private List<UbisoftCatalogItem> _catalog = new();

    public IEnumerable<UbisoftCatalogItem> Catalog => _catalog;

    public UbisoftCatalog(string launcherPath)
    {
        _catalogPath = Path.Combine(launcherPath, "cache", "configuration", "configurations");
        Refresh();
    }

    public void Refresh()
    {
        List<UbisoftCatalogItem> deserializedUbisoftCatalogList = new();

        if (!File.Exists(_catalogPath))
            throw new FileNotFoundException("Configuration file not found, probably UPC client hasn't been started at least once.", _catalogPath);

        using var file = File.OpenRead(_catalogPath);

        var catalogCollection = ProtoBuf.Serializer.Deserialize<UbisoftCatalogCollection>(file);

        if (catalogCollection is null || catalogCollection.Games is null)
            throw new InvalidDataException("Error while parsing catalog file.");

        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        foreach (var item in catalogCollection.Games.Where(p => !string.IsNullOrEmpty(p.GameInfoYaml)))
        {
            try
            {
                item.GameInfo = deserializer.Deserialize<UbisoftProductInformation>(item.GameInfoYaml!);
            }
            catch { continue; }
            deserializedUbisoftCatalogList.Add(item);
        }

        _catalog = deserializedUbisoftCatalogList;
    }
}
