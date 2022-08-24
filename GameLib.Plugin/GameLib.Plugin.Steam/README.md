![GameLib.NET](Resources/GameLibPluginLogo64px.png "GameLib.NET Steam") 
Steam Plugin for GameLib.NET
======

[Steam](https://store.steampowered.com) plugin for ![GameLib.NET](../../Resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md).

The plugin will deliver information about the installation status of the `Steam` launcher as well the installed games within the launcher.

## Installing

The plugin is already bundled with the core library ![GameLib.NET](../../Resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md)

## Additional information the plugin is providing

To get the additonal information this plugin is providing just cast `ILauncher` to `SteamLauncher` and `IGame` to `SteamGame`.


```CSharp
using GameLib;
using GameLib.Plugin.Steam;
using GameLib.Plugin.Steam.Model;

var launcherManager = new LauncherManager();

var launcher = (SteamLauncher?)launcherManager.GetLaunchers()
    .Where(launcher => launcher.Name == "Steam")
    // GUID of the class could also be used instead of the name
    //.Where(launcher => launcher.GetType().GUID == new Guid("5BB973D0-BF3D-4C3E-98B2-41AEFCB1506A"))
    .FirstOrDefault();

if (launcher is not null)
{
    // Steam launcher provides libraries (games can reside on different disks)
    var libs = launcher.GetLibraries();
    foreach (var lib in libs)
    {
        Console.WriteLine($"\nLibrary");
        Console.WriteLine($"\tName: {lib.Name}");
        Console.WriteLine($"\tPath: {lib.Path}");
        Console.WriteLine($"\tContentId: {lib.ContentId}");
        Console.WriteLine($"\tTotalSize: {lib.TotalSize}");
    }

    var games = (IEnumerable<SteamGame>)launcher.GetGames();
    foreach (var game in games)
    {
        // Write addtional data Steam is providing for a game besides from the IGame inteface
        Console.WriteLine($"\nGame");
        Console.WriteLine($"\tUniverse: {game.Universe}");
        Console.WriteLine($"\tLastUpdated: {game.LastUpdated}");
        Console.WriteLine($"\tSizeOnDisk: {game.SizeOnDisk}");
        Console.WriteLine($"\tLastOwner: {game.LastOwner}");
        Console.WriteLine($"\tAutoUpdateBehavior: {game.AutoUpdateBehavior}");
        Console.WriteLine($"\tAllowOtherDownloadsWhileRunning: {game.AllowOtherDownloadsWhileRunning}");
        Console.WriteLine($"\tScheduledAutoUpdate: {game.ScheduledAutoUpdate}");
        Console.WriteLine($"\tIsUpdating: {game.IsUpdating}");
        Console.WriteLine($"\tDeveloper: {game.Developer}");
        Console.WriteLine($"\tDeveloperUrl: {game.DeveloperUrl}");
        Console.WriteLine($"\tPublisher: {game.Publisher}");
        Console.WriteLine($"\tHomepage: {game.Homepage}");
        Console.WriteLine($"\tGameManualUrl: {game.GameManualUrl}");
    }
}
```