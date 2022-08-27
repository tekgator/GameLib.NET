# Changelog
All notable changes to this project will be documented in this file.

## [Unreleased]


## [1.1.1] - 2022-27-08
### Fixed
- Refresh caused an dead lock in UI applications due to incorrect async call
- Refresh on LauncherManager also returns the Launchers collection so no extra GetLaunchers call need to be made


## [1.1.0] - 2022-27-08
### Fixed
- disable 0649 to avoid compiler warning for MEF variables

### Added
- Add LauncherOptions to ILauncher interface to change settings on the fly for a single plugin
- Add Documentation to LauncherOptions
- Add Documentation to LauncherManager methods 

### Changed
- Corrected Plugin documentation concerning referencing the Plugin ID
- Restructure of code
- Rename ClearCache method on LauncherManager and ILauncher Interface to Refresh
- LauncherManager will now always refresh all Plugins including the Games on initial GetLauncher call
- Remove GetGames method on ILauncher interface and instead add Games property


## [1.0.6] - 2022-24-08
### Fixed
- Origin plugin did not return working directory and executable name for online queries

### Changed
- Launchers property replaced with GetLaunchers method on LauncherManager as a property should not be an expensive call. As on first call the plugins are loaded the property can be executing for a bit depending on the system.


## [1.0.5] - 2022-13-08
### Added
- Add ID property on ILauncher interface
- Add Launcher ID property on IGame interface in order to know what launcher the game belongs to

### Changed
- Rename LargeLogo property to Logo on ILauncher interface
- Simplify console demo by writing every property of the ILauncher interface

### Deleted
- Remove SmallLogo property on ILauncher interface


## [1.0.4] - 2022-12-08
### Added
- Add LargeLogo property on ILauncher interface

### Changed
- Rename Icon property to SmallLogo on ILauncher interface


## [1.0.3] - 2022-10-08
### Added
- Changelog file
- Add changelog file to Nuget
- 32px game launcher icons
- Provide Icon property on ILauncher interface

### Changed
- Restructioring of the project by removing SRC folder
- Demo executable icon is loaded from Ressource folder instead of keeping it with the project
- Add game launcher icons to ReadMe
- Rearrange resource files from solution into plugin resource folders


## [1.0.2] - 2022-10-07
### Added
- Add reference to Teronis.DotNet to be able to add project reference content to be added to the NuGet-package during pack process

### Changed
- Plugins will not create own Nuget anymore
- Plugins are now bundled with the core library


## [1.0.1] - 2022-10-07
### Added
- Add automatic version for Nuget publishing process


## [1.0.0] - 2022-10-06
### Added
- Add automtic creation of Nuget


## [0.9.0] - 2022-10-05
### Added
- First working version



This project is MIT Licensed // Created & maintained by Patrick Weiss