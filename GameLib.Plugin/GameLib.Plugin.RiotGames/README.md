![GameLib.NET](Resources/GameLibPluginLogo128px.png "GameLib.NET Riot Games")
Riot Games Plugin for GameLib.NET
======

[Riot Games](https://www.riotgames.com/en) plugin for ![GameLib.NET](../../Resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md).

The plugin will deliver information about the installation status of the `Riot Games` launcher as well the installed games within the launcher.

## Installing

The plugin is already bundled with the core library ![GameLib.NET](../../Resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md)

## Additional information the plugin is providing

To get the additonal information this plugin is providing just cast `IGame` to `GogGame`.


```CSharp
using GameLib;
using GameLib.Plugin.RiotGames;
using GameLib.Plugin.RiotGames.Model;

var launcherManager = new LauncherManager();

// not required to cast here just to add to the documentation
var launcher = (RiotGamesLauncher?)launcherManager.GetLaunchers()
    .Where(launcher => launcher.Name == "Riot Games")
    // Plugin ID could also be used instead of the name
    //.Where(launcher => launcher.Id == new Guid("938071a9-925e-4b1d-ae17-61a05ef5ec28"))
    .FirstOrDefault();

if (launcher is not null)
{
    var games = (IEnumerable<GogGame>)launcher.Games;
    foreach (var game in games)
    {
        
        Console.WriteLine($"Game ID: {game.GameId}");
        foreach (var item in game.GetType().GetProperties().Where(p => p.Name != "GameId"))
        {
            Console.WriteLine($"\t{item.Name}: {item.GetValue(game)}");
        }
    }
}
```