using GameLauncherApi.Ubisoft.Model;
using YamlDotNet.Serialization;

namespace GameLib.Plugin.Ubisoft;

internal class UbisoftCatalogue
{
    private readonly string _cataloguePath;
    private List<UbisoftCatalogueItem> _catalogue = new();

    public List<UbisoftCatalogueItem> Catalogue => _catalogue;

    public UbisoftCatalogue(string launcherPath)
    {
        _cataloguePath = Path.Combine(launcherPath, "cache", "configuration", "configurations");
        Refresh();
    }

    public void Refresh()
    {
        List<UbisoftCatalogueItem> deserializedUbisoftCatalogueList = new();

        if (!File.Exists(_cataloguePath))
            throw new FileNotFoundException("Configuration file not found, probalby UPC client hasn't been started at least once.", _cataloguePath);

        using var file = File.OpenRead(_cataloguePath);

        var catalogueCollection = ProtoBuf.Serializer.Deserialize<UbisoftCatalogueCollection>(file);

        if (catalogueCollection is null || catalogueCollection.Games is null)
            throw new InvalidDataException("Error while parsing catalogue file.");

        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        foreach (var item in catalogueCollection.Games.Where(p => !string.IsNullOrEmpty(p.GameInfoYaml)))
        {
            try
            {
                item.GameInfo = deserializer.Deserialize<UbisoftProductInformation>(item.GameInfoYaml!);
            }
            catch { continue; }
            deserializedUbisoftCatalogueList.Add(item);
        }

        _catalogue = deserializedUbisoftCatalogueList;
    }

}
