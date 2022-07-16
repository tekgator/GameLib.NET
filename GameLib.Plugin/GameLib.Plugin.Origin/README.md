![GameLib.NET](../../Resources/GameLibNET-Origin-Logo-64px.png "GameLib.NET Origin") 
Origin Plugin for GameLib.NET
======

[Origin](https://www.origin.com) plugin for ![GameLib.NET](../../Resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md).

The plugin will deliver information about the installation status of the `Origin` launcher as well the installed games within the launcher.

## Installing

The plugin is already bundled with the core library ![GameLib.NET](../../Resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md)

## Additional information the plugin is providing

To get the additonal information this plugin is providing just cast `IGame` to `OriginGame`.


```CSharp
using GameLib;
using GameLib.Plugin.Origin;
using GameLib.Plugin.Origin.Model;

var launcherManager = new LauncherManager();

// not required to cast here just to add to the documentation
var launcher = (OriginLauncher?)launcherManager.Launchers
    .Where(launcher => launcher.Name == "Origin")
    // GUID of the class could also be used instead of the name
    //.Where(launcher => launcher.GetType().GUID == new Guid("54C9D299-107E-4990-894D-9DB402F81CA3"))
    .FirstOrDefault();

if (launcher is not null)
{
    var games = (IEnumerable<OriginGame>)launcher.GetGames();
    foreach (var game in games)
    {
        // Write addtional data Origin is providing for a game besides from the IGame inteface
        Console.WriteLine($"\nGame");
        Console.WriteLine($"\tLocale: {game.Locale}");
        Console.WriteLine($"\tTotalBytes: {game.TotalBytes}");
    }
}
```