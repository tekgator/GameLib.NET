![GameLib.NET](resources/GameLibNET-Logo-64px.png "GameLib.NET") 
GameLib.NET
======

GameLib.NET is a library to give .NET developers easy access to the users installed game launchers and installed games. The motivation for the library is a tool I'm currently working on which requires access to all game executables on a PC.

While this repository is providing already the plugins to gather the games from the most popular game launchers, it easily extendible via the MEF Framework. A developer guide will follow, but I'm pretty sure the geeks will find out themselfes on how to do it.

Following plugins are available to detect the game launchers including their installed games:
- [Steam](https://store.steampowered.com/)
- [Epic Games](https://store.epicgames.com)
- [Ubisoft Connect](https://ubisoftconnect.com/)
- [Origin](https://www.origin.com/)
- [GOG Galaxy 2.0](https://www.gog.com/galaxy)

## Installing

Install, using the [Nuget Gallery](https://www.nuget.org/packages?q=tekgator+GameLib.net), the corrosponding launcher plugins you like to have. Since the actual core is a dependency it will install it automatically.

You can also use the following command in the Package Manager Console:
```ps
Install-Package GameLib.NET
```

## Usage

GameLib.NET provides a `LauncherManager` class which has to be instantiated, optionally `LauncherOptions` can be supplied. Each Plugin will provide an interface for the launcher `ILauncher` as well an interface for `IEnumerable<Game>`.

```CSharp
using GameLib;

var launcherManager = new LauncherManager(new LauncherOptions() { QueryOnlineData = true });

foreach (var launcher in launcherManager.Launchers)
{
    Console.WriteLine($"Launcher name: {launcher.Name}");
    Console.WriteLine("Games:");

    foreach (var game in launcher.GetGames())
    {
        Console.WriteLine($"Game ID: {game.GameId}");
        foreach (var item in game.GetType().GetProperties().Where(p => p.Name != "GameId"))
        {
            Console.WriteLine($"\t{item.Name}: {item.GetValue(game)}");
        }
    }
}

```


## Support

I try to be responsive to [Stack Overflow questions in the `gamelib-net` tag](https://stackoverflow.com/questions/tagged/gamelib-net) and [issues logged on this GitHub repository](https://github.com/tekgator/GameLib.NET/issues). 

If I've helped you, feel free to buy me a coffee or see the Sponsor link [at the top right of the GitHub page](https://github.com/tekgator/GameLib.NET).

<a href="https://www.buymeacoffee.com/tekgator" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>

## Dependencies and Credits

- Icons are created by [Flat Icons](https://www.flaticon.com)

- Thanks to [Josef Nemec](https://github.com/JosefNemec) and contributers of [Playnite](https://github.com/JosefNemec/Playnite) for the inspiration of decoding the proprietary manifest and catalog data of each launcher 

- The team of [SteamDB](https://steamdb.info) providing [Valve's KeyValue for .NET](https://github.com/SteamDatabase/ValveKeyValue) for reading Steam's proprietary key value format files

- JSON deserializing by [Json.NET](https://www.newtonsoft.com/json)</a>

- YAML deserializing by [YamlDotNet](https://github.com/aaubry/YamlDotNet)

- [protobuf-net](https://github.com/protobuf-net/protobuf-net) for Protobuffer reading for decoding Ubisoft's local catalog data
