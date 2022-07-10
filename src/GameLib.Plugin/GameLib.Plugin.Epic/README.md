![GameLib.NET](../../../resources/GameLibNET-Epic-Logo-64px.png "GameLib.NET Epic Games") 
Epic Games Plugin for GameLib.NET
======

`Epic Games` plugin for ![GameLib.NET](../../../resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md), Please check out the core documention for first.

The plugin will deliver information about the installation status of the `Epic Games` launcher as well the installed games within the launcher.

## Installing

Install, using the [Nuget Gallery](https://www.nuget.org/packages?q=GameLib.NET.Epic).

You can also use the following command in the Package Manager Console:
```ps
Install-Package GameLib.NET.Epic
```

## Additional information the plugin is providing

To get the additonal information this plugin is providing just cast `IGame` to `EpicGame`.


```CSharp
using GameLib;
using GameLib.Plugin.Epic;
using GameLib.Plugin.Epic.Model;

var launcherManager = new LauncherManager();

// not required to cast here just to add to the documentation
var launcher = (EpicLauncher?)launcherManager.Launchers
    .Where(launcher => launcher.Name == "Epic Games")
    // GUID of the class could also be used instead of the name
    //.Where(launcher => launcher.GetType().GUID == new Guid("282B9BB6-54CA-4293-83CF-6F1134CDEEC6"))
    .FirstOrDefault();

if (launcher is not null)
{
    var games = (IEnumerable<EpicGame>)launcher.GetGames();
    foreach (var game in games)
    {
        // Write addtional data Epic Games is providing for a game besides from the IGame inteface
        Console.WriteLine($"\nGame");
        Console.WriteLine($"\tInstallSize: {game.InstallSize}");
        Console.WriteLine($"\tVersion: {game.Version}");
    }
}
```