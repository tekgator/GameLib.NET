using System.Drawing;

namespace GameLib.Core;

public interface IGame
{
    /// <summary>
    /// Unique game Id if the launcher provides it
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Guid of the Launcher
    /// </summary>
    public Guid LauncherId { get; }

    /// <summary>
    /// Name of the Game
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Installation dir of the game
    /// </summary>
    public string InstallDir { get; }

    /// <summary>
    /// Executable name including the full Path
    /// </summary>
    public string ExecutablePath { get; }

    /// <summary>
    /// Just the executable name
    /// </summary>
    public string Executable { get; }

    /// <summary>
    /// The extracted icon of the game executable
    /// </summary>
    public Icon? ExecutableIcon { get; }

    /// <summary>
    /// Working directory of the game
    /// </summary>
    public string WorkingDir { get; }

    /// <summary>
    /// The launch string of the game. Can be used with Process.Start() to start the game
    /// </summary>
    public string LaunchString { get; }

    /// <summary>
    /// When the game got installed; if the launcher cannot provide the information the creation date of the InstallDir is returned
    /// </summary>
    public DateTime InstallDate { get; }

    /// <summary>
    /// Check whether the game is currently running (works only if launch executable is also the game executable)
    /// e.g. GTA5 is launched with PlayGTAV.exe but the actual game is running via GTAV.exe
    /// </summary>
    public bool IsRunning { get; }
}
