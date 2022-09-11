using Gamelib.Core.Util;
using GameLib.Core;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.BattleNet;

[Guid("3BF9899A-AF88-4935-893C-3B99A577A565")]
[Export(typeof(ILauncher))]
public class BattleNetLauncher : ILauncher
{
    [ImportingConstructor]
    public BattleNetLauncher(LauncherOptions? launcherOptions)
    {
        LauncherOptions = launcherOptions ?? new LauncherOptions();
    }

    #region Interface implementations
    public LauncherOptions LauncherOptions { get; }

    public Guid Id => GetType().GUID;

    public string Name => "Battle.net";

    public Image Logo => Properties.Resources.Logo128px;

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);

    public string InstallDir { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public Icon? ExecutableIcon => PathUtil.GetFileIcon(Executable);

    public IEnumerable<IGame> Games { get; private set; } = Enumerable.Empty<IGame>();

    public void Refresh(CancellationToken cancellationToken = default)
    {
        Executable = string.Empty;
        InstallDir = string.Empty;
        IsInstalled = false;
        Games = Enumerable.Empty<IGame>();

        Executable = GetExecutable() ?? string.Empty;
        if (!string.IsNullOrEmpty(Executable))
        {
            InstallDir = Path.GetDirectoryName(Executable) ?? string.Empty;
            IsInstalled = File.Exists(Executable);
            Games = BattleNetGameFactory.GetGames(this, cancellationToken);
        }
    }

    public bool Start() => IsRunning || ProcessUtil.StartProcess(Executable);

    public void Stop()
    {
        if (IsRunning)
        {
            Process.Start(Executable, "--exec=shutdown");
        }
    }
    #endregion

    #region Private methods
    private static string? GetExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("battlenet");
        executablePath ??= RegistryUtil.GetShellCommand("blizzard");
        executablePath ??= RegistryUtil.GetShellCommand("Blizzard.URI.Battlenet");
        executablePath ??= RegistryUtil.GetShellCommand("Blizzard.URI.Blizzard");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!PathUtil.IsExecutable(executablePath))
        {
            executablePath = null;
        }

        return executablePath;
    }
    #endregion
}