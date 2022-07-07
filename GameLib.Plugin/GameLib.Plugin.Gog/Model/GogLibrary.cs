namespace GameLib.Plugin.Gog.Model;

public class GogLibrary : ILibrary
{
    public string Name { get; internal set; } = string.Empty;
    public string Path { get; internal set; } = string.Empty;
}
