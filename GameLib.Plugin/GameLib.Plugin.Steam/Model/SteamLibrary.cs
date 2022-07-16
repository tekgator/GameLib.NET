namespace GameLib.Plugin.Steam.Model;

public class SteamLibrary
{
    public string Name { get; internal set; } = string.Empty;
    public string Path { get; internal set; } = string.Empty;
    public long ContentId { get; internal set; } = 0;
    public long TotalSize { get; internal set; } = 0;
}
