using Gamelib.Core.Util;
using GameLib.Core;
using GameLib.Plugin.Steam.Model;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Steam;

[Guid("5BB973D0-BF3D-4C3E-98B2-41AEFCB1506A")]
[Export(typeof(ILauncher))]
public class SteamLauncher : ILauncher
{
    [ImportingConstructor]
    public SteamLauncher(LauncherOptions? launcherOptions)
    {
        LauncherOptions = launcherOptions ?? new LauncherOptions();
    }

    #region Interface implementations
    public LauncherOptions LauncherOptions { get; }

    public Guid Id => GetType().GUID;

    public string Name => "Steam";

    public Image Logo => Properties.Resources.Logo512px;

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(ExecutablePath);

    public string InstallDir { get; private set; } = string.Empty;

    public string ExecutablePath { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public Icon? ExecutableIcon => PathUtil.GetFileIcon(ExecutablePath);

    public IEnumerable<IGame> Games { get; private set; } = Enumerable.Empty<IGame>();

    public bool Start() => IsRunning || ProcessUtil.StartProcess(ExecutablePath);

    public void Stop()
    {
        if (IsRunning)
        {
            Process.Start(ExecutablePath, "-shutdown");
        }
    }

    public void Refresh(CancellationToken cancellationToken = default)
    {
        ExecutablePath = string.Empty;
        Executable = string.Empty;
        InstallDir = string.Empty;
        IsInstalled = false;
        Libraries = Enumerable.Empty<SteamLibrary>();
        Games = Enumerable.Empty<IGame>();

        ExecutablePath = GetExecutable() ?? string.Empty;
        if (!string.IsNullOrEmpty(ExecutablePath))
        {
            Executable = Path.GetFileName(ExecutablePath);
            InstallDir = Path.GetDirectoryName(ExecutablePath) ?? string.Empty;
            IsInstalled = File.Exists(ExecutablePath);
            Libraries = SteamLibraryFactory.GetLibraries(InstallDir);
            Games = SteamGameFactory.GetGames(this, Libraries);
        }
    }
    #endregion

    #region Public properties
    public IEnumerable<SteamLibrary> Libraries { get; private set; } = Enumerable.Empty<SteamLibrary>();
    #endregion

    #region Private methods
    private static string? GetExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("steam");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.CurrentUser, @"Software\Valve\Steam", "SteamExe");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.CurrentUser, @"Software\Valve\Steam", "SteamPath");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"Software\Valve\Steam", "InstallPath");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!string.IsNullOrEmpty(executablePath) && !PathUtil.IsExecutable(executablePath))
        {
            executablePath = Path.Combine(executablePath, "steam.exe");
        }

        if (!PathUtil.IsExecutable(executablePath))
        {
            executablePath = null;
        }

        return executablePath;
    }
    #endregion

}