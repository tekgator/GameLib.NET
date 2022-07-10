![GameLib.NET](../../../resources/GameLibNET-Gog-Logo-64px.png "GameLib.NET GOG Galaxy") 
GOG Galaxy Plugin for GameLib.NET
======

`GOG Galaxy` plugin for ![GameLib.NET](../../../resources/GameLibNET-Logo-16px.png "GameLib.NET") [GameLib.NET](README.md), Please check out the core documention for first.

The plugin will deliver information about the installation status of the `GOG Galaxy` launcher as well the installed games within the launcher.

## Installing

Install, using the [Nuget Gallery](https://www.nuget.org/packages?q=GameLib.NET.Gog).

You can also use the following command in the Package Manager Console:
```ps
Install-Package GameLib.NET.Gog
```

## Additional information the plugin is providing

To get the additonal information this plugin is providing just cast `IGame` to `GogGame`.


```CSharp
using GameLib;
using GameLib.Plugin.Gog;
using GameLib.Plugin.Gog.Model;

var launcherManager = new LauncherManager();

// not required to cast here just to add to the documentation
var launcher = (GogLauncher?)launcherManager.Launchers
    .Where(launcher => launcher.Name == "GOG Galaxy")
    // GUID of the class could also be used instead of the name
    //.Where(launcher => launcher.GetType().GUID == new Guid("54C9D299-107E-4990-894D-9DB402F81CA3"))
    .FirstOrDefault();

if (launcher is not null)
{
    var games = (IEnumerable<GogGame>)launcher.GetGames();
    foreach (var game in games)
    {
        // Write addtional data GOG Galaxy is providing for a game besides from the IGame inteface
        Console.WriteLine($"\nGame");
        Console.WriteLine($"\tBuildId: {game.BuildId}");
        Console.WriteLine($"\tDependsOn: {game.DependsOn}");
        Console.WriteLine($"\tDlc: {game.Dlc}");
        Console.WriteLine($"\tInstallerLanguage: {game.InstallerLanguage}");
        Console.WriteLine($"\tLangCode: {game.LangCode}");
        Console.WriteLine($"\tLanguage: {game.Language}");
        Console.WriteLine($"\tLaunchCommand: {game.LaunchCommand}");
        Console.WriteLine($"\tLaunchParam: {game.LaunchParam}");
        Console.WriteLine($"\tPath: {game.Path}");
        Console.WriteLine($"\tProductId: {game.ProductId}");
        Console.WriteLine($"\tStartMenu: {game.StartMenu}");
        Console.WriteLine($"\tStartMenuLink: {game.StartMenuLink}");
        Console.WriteLine($"\tSupportLink: {game.SupportLink}");
        Console.WriteLine($"\tUninstallCommand: {game.UninstallCommand}");
        Console.WriteLine($"\tVersion: {game.Version}");
    }
}
```