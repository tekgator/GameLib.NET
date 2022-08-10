using System.Drawing;

namespace GameLib.Core;

public interface ILauncher
{
    /// <summary>
    /// Name of the launcher
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Icon of the launcher
    /// </summary>
    public Image? Icon { get; }

    /// <summary>
    /// <see langword="true"/> if the launcher is (properly) installed
    /// </summary>
    public bool IsInstalled { get; }

    /// <summary>
    /// <see langword="true"/> if the launcher is currently running
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// The installation path of the Launcher<br/>
    /// <see langword="string.Empty"/> if not (properly) installed
    /// </summary>
    public string InstallDir { get; }

    /// <summary>
    /// The executable including the path of the Launcher<br/>
    /// <see langword="string.Empty"/> if not (properly) installed
    /// </summary>
    public string ExecutablePath { get; }

    /// <summary>
    /// The executable name of the Launcher<br/>
    /// <see langword="string.Empty"/> if not (properly) installed
    /// </summary>
    public string Executable { get; }

    /// <summary>
    /// The installed games of the Launcher
    /// </summary>
    public IEnumerable<IGame> GetGames(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the launcher if not already running
    /// </summary>
    /// <returns><see langword="True"/> if started successfully or already running</returns>
    public bool Start();

    /// <summary>
    /// Attempts to end the launcher
    /// </summary>
    public void Stop();

    /// <summary>
    /// Clear any cached values so those get refreshed on next access
    /// </summary>
    public void ClearCache();
}