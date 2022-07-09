namespace GameLib;

public class LauncherOptions
{
    public bool LoadLocalCatalogData { get; init; } = true;
    public bool QueryOnlineData { get; init; } = true;
    public TimeSpan? OnlineQueryTimeout { get; init; } = null;
}
