![GameLib.NET](../../../resources/GameLibNET-Ubisoft-Logo-64px.png "GameLib.NET Ubisoft") 
Ubisoft Connect Plugin for GameLib.NET
======

`Ubisoft connect` plugin for ![GameLib.NET](../../../resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md), Please check out the core documention for first.

The plugin will deliver information about the installation status of the `Ubisoft Connect` launcher as well the installed games within the launcher.

## Installing

Install, using the [Nuget Gallery](https://www.nuget.org/packages?q=GameLib.NET.Ubisoft).

You can also use the following command in the Package Manager Console:
```ps
Install-Package GameLib.NET.Ubisoft
```

## Additional information the plugin is providing

To get the additonal information this plugin is providing just cast `IGame` to `UbisoftGame`.


```CSharp
using GameLib;
using GameLib.Plugin.Ubisoft;
using GameLib.Plugin.Ubisoft.Model;

var launcherManager = new LauncherManager();

// not required to cast here just to add to the documentation
var launcher = (UbisoftLauncher?)launcherManager.Launchers
    .Where(launcher => launcher.Name == "Ubisoft Connect")
    // GUID of the class could also be used instead of the name
    //.Where(launcher => launcher.GetType().GUID == new Guid("CE276C05-6CD1-4D99-9A5A-2E03ECFB6028"))
    .FirstOrDefault();

if (launcher is not null)
{
    var games = (IEnumerable<UbisoftGame>)launcher.GetGames();
    foreach (var game in games)
    {
        // Write addtional data Ubisoft Connect is providing for a game besides from the IGame inteface
        Console.WriteLine($"\nGame");
        Console.WriteLine($"\tLanguage: {game.Language}");
        Console.WriteLine($"\tHelpUrl: {game.HelpUrl}");
        Console.WriteLine($"\tFacebookUrl: {game.FacebookUrl}");
        Console.WriteLine($"\tHomepageUrl: {game.HomepageUrl}");
        Console.WriteLine($"\tForumUrl: {game.ForumUrl}");
    }
}
```