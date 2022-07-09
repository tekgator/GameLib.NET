namespace GameLib.Api;

public class LauncherOptions
{
    /// <summary>
    /// Some launcher download a catalog of their available games in form of a cached file.
    /// Those files can be processed to gather further information about the game, e.g.
    /// Game name, Executables, URL's,etc.
    /// Since those files could be a bit large loading of those files can be controlled
    /// with this option.
    /// </summary>
    public bool LoadLocalCatalogData { get; init; } = true;

    /// <summary>
    /// Some launcher don't provide all the necessary game data in a manifest or catalog
    /// file, but provide a online API call to gather information, e.g.
    /// Game name, Executables, URL's,etc.
    /// Since this option could be time consuming it can be controlled with this property
    /// </summary>
    public bool QueryOnlineData { get; init; } = true;

    /// <summary>
    /// Default online query time out of 100s can be overwritten with this option
    /// </summary>
    public TimeSpan? OnlineQueryTimeout { get; init; } = null;
}
