namespace GameLib.Plugin.Steam.Model;

public class DeserializedSteamLibrary
{
    public string? Path { get; set; }
    public string? Label { get; set; }
    public long ContentId { get; set; }
    public long TotalSize { get; set; }

    public SteamLibrary SteamLibraryBuilder() => new()
    {
        Name = Label ?? string.Empty,
        Path = Path ?? string.Empty,
        ContentId = ContentId,
        TotalSize = TotalSize
    };
}
