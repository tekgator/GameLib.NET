![GameLib.NET](Resources/GameLibPluginLogo64px.png "GameLib.NET Battle.net") 
Battle.net Plugin for GameLib.NET
======

[Battle.net connect](https://battle.net) plugin for ![GameLib.NET](../../Resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md).

The plugin will deliver information about the installation status of the `Battle.net` launcher as well the installed games within the launcher.

## Installing

The plugin is already bundled with the core library ![GameLib.NET](../../Resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md)

## Special notes on the plugin

Battle.net has a local installed product database. Unfortunettally this database does not have any information about a game name or the executables.
To get around this problem a [JSON file](Resources/BattleNetGames.json) is provided with the plugin to have this information available.

Please feel free to contribute on how to get around this problem. Until then we have to live with the JSON file.

*Source: [Steam forum](https://steamcommunity.com/sharedfiles/filedetails/?id=1113049716")

## Additional information the plugin is providing

To get the additonal information this plugin is providing just cast `IGame` to `BattleNetGame`.


```CSharp
using GameLib;
using GameLib.Plugin.BattleNet;
using GameLib.Plugin.BattleNet.Model;

var launcherManager = new LauncherManager();

// not required to cast here just to add to the documentation
var launcher = (BattleNetLauncher?)launcherManager.GetLaunchers()
    .Where(launcher => launcher.Name == "Battle.net")
    // Plugin ID could also be used instead of the name
    //.Where(launcher => launcher.Id == new Guid("3BF9899A-AF88-4935-893C-3B99A577A565"))
    .FirstOrDefault();

if (launcher is not null)
{
    var games = (IEnumerable<BattleNetGame>)launcher.Games;
    foreach (var game in games)
    {
        // Write additional data Battle.Net is providing for a game besides from the IGame interface
        Console.WriteLine($"\nGame");
        Console.WriteLine($"\tProductCode: {game.ProductCode}");
        Console.WriteLine($"\tSpeechLanguage: {game.SpeechLanguage}");
        Console.WriteLine($"\tTextLanguage: {game.TextLanguage}");
        Console.WriteLine($"\tPlayRegion: {game.PlayRegion}");
        Console.WriteLine($"\tVersion: {game.Version}");
    }
}
```