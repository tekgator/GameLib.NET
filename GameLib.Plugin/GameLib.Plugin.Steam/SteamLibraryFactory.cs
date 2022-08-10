using Gamelib.Core.Util;
using GameLib.Plugin.Steam.Model;
using ValveKeyValue;

namespace GameLib.Plugin.Steam;

internal static class SteamLibraryFactory
{
    public static IEnumerable<SteamLibrary> GetLibraries(string installDir)
    {
        var libraryVdfPath = Path.Combine(installDir, "config", "libraryfolders.vdf");

        if (!File.Exists(libraryVdfPath))
            return Enumerable.Empty<SteamLibrary>();

        using var stream = File.OpenRead(libraryVdfPath);
        var serializer = KVSerializer.Create(KVSerializationFormat.KeyValues1Text);

        var deserializedLibraries = serializer.Deserialize<Dictionary<string, DeserializedSteamLibrary>>(stream);

        return deserializedLibraries
            .Select(lib => lib.Value)
            .Select(lib =>
            {
                lib.Path = PathUtil.Sanitize(lib.Path) ?? string.Empty;
                return lib.SteamLibraryBuilder();
            })
            .ToList();
    }
}
