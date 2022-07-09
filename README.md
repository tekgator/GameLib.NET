![GameLib.NET](resources/logos/logo-color-64px.png "GameLib.NET") 
GameLib.NET
======

The project is maintained by [Patrick Weiss](https://github.com/tekgator). Problems and issues can be filed on the [Github repository](https://github.com/tekgator/GameLib.NET/issues)

## Description

GameLib.NET is a library to give .NET developers easy access to the users installed game launchers and installed games. The motivation for the library is a tool I'm currently working on which requires access to all game executables on a PC.

While this repository is providing already the tools to gather the games from the most common game launchers, it easily extendible via the MEF Framework.

The base library detects  following game launchers including their installed games:
- [Steam](https://store.steampowered.com/)
- [Epic Games](https://store.epicgames.com)
- [Ubisoft Connect](https://ubisoftconnect.com/)
- [Origin](https://www.origin.com/)
- [GOG Galaxy 2.0](https://www.gog.com/galaxy)

## buy-me-a-coffee
Like some of my work? Buy me a coffee ☕ (or more likely a beer 🍺, or even more likly shoes 👠 or purse 👜 for the wify 😄)

<a href="https://www.buymeacoffee.com/tekgator" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>


## Dependencies and Credits

- Icons are created by [Flat Icons](https://www.flaticon.com)

- Teneko's [Teronis.DotNet](https://github.com/teneko/Teronis.DotNet/tree/develop/src/MSBuild/Packaging/ProjectBuildInPackage) which allows project reference content to be added to the NuGet-package during pack process

- The team of [SteamDB](https://steamdb.info) providing [Valve's KeyValue for .NET](https://github.com/SteamDatabase/ValveKeyValue) for reading Steam's proprietary key value format files

- JSON deserializing by [Json.NET](https://www.newtonsoft.com/json)</a>

- YAML deserializing by [YamlDotNet](https://github.com/aaubry/YamlDotNet)

- [protobuf-net](https://github.com/protobuf-net/protobuf-net) for Protobuffer reading for decoding Ubisoft's local catalog data

- Thanks to [Josef Nemec](https://github.com/JosefNemec) and contributers of [Playnite](https://github.com/JosefNemec/Playnite) for the inspiration of decoding the proprietary manifest and catalog data of each launcher 
