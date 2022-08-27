using Gamelib.Core.Util;
using GameLib.Core;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Gog;

[Guid("02BBDAB7-68D7-4CE1-9044-934DFEC6CF17")]
[Export(typeof(ILauncher))]
public class GogLauncher : ILauncher
{
    [ImportingConstructor]
    public GogLauncher(LauncherOptions? launcherOptions)
    {
        LauncherOptions = launcherOptions ?? new LauncherOptions();
    }

    #region Interface implementations
    public LauncherOptions LauncherOptions { get; }

    public Guid Id => GetType().GUID;

    public string Name => "GOG Galaxy";

    public Image Logo => Properties.Resources.Logo128px;

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public IEnumerable<IGame> Games { get; private set; } = Enumerable.Empty<IGame>();

    public void Refresh(CancellationToken cancellationToken = default)
    {
        ExecutablePath = string.Empty;
        Executable = string.Empty;
        InstallDir = string.Empty;
        IsInstalled = false;
        Games = Enumerable.Empty<IGame>();

        ExecutablePath = GetExecutable() ?? string.Empty;
        if (!string.IsNullOrEmpty(ExecutablePath))
        {
            Executable = Path.GetFileName(ExecutablePath);
            InstallDir = Path.GetDirectoryName(ExecutablePath) ?? string.Empty;
            IsInstalled = File.Exists(ExecutablePath);
            Games = GogGameFactory.GetGames(this, cancellationToken);
        }
    }

    public bool Start() => IsRunning || ProcessUtil.StartProcess(ExecutablePath);

    public void Stop()
    {
        if (IsRunning)
        {
            Process.Start(ExecutablePath, "/command=shutdown");
        }
    }
    #endregion

    #region Private methods
    private static string? GetExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("goggalaxy");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\GOG.com\GalaxyClient\paths", "client");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!string.IsNullOrEmpty(executablePath) && !PathUtil.IsExecutable(executablePath))
        {
            executablePath = Path.Combine(executablePath, RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\GOG.com\GalaxyClient", "clientExecutable") ?? string.Empty);
        }

        if (!PathUtil.IsExecutable(executablePath))
        {
            executablePath = null;
        }

        return executablePath;
    }
    #endregion

}