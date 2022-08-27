namespace GameLib.Core;

public class LauncherOptions
{
    /// <summary>
    /// Define if Launcher plugin should use local catalog data to get more detailed information about specific games
    /// Note: Can in increase load time for the plugin
    /// </summary>
    public bool LoadLocalCatalogData { get; init; } = true;

    /// <summary>
    /// Define if Launcher plugin should use online query's to get more detailed information about specific games
    /// Note: Can in increase load time for the plugin
    /// </summary>
    public bool QueryOnlineData { get; init; } = true;

    /// <summary>
    /// Defines the timeout time for online query's
    /// </summary>
    public TimeSpan? OnlineQueryTimeout { get; init; } = null;
}
