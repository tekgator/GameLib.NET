using Gamelib.Core.Util;
using GameLib.Core;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Epic;

[Guid("282B9BB6-54CA-4293-83CF-6F1134CDEEC6")]
[Export(typeof(ILauncher))]
public class EpicLauncher : ILauncher
{
    [ImportingConstructor]
    public EpicLauncher(LauncherOptions? launcherOptions)
    {
        LauncherOptions = launcherOptions ?? new LauncherOptions();
    }

    #region Interface implementations
    public LauncherOptions LauncherOptions { get; }

    public Guid Id => GetType().GUID;

    public string Name => "Epic Games";

    public Image Logo => Properties.Resources.Logo128px;

    public bool IsInstalled { get; private set; }

    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);

    public string InstallDir { get; private set; } = string.Empty;

    public string Executable { get; private set; } = string.Empty;

    public Icon? ExecutableIcon => PathUtil.GetFileIcon(Executable);

    public IEnumerable<IGame> Games { get; private set; } = Enumerable.Empty<IGame>();

    public bool Start() => IsRunning || ProcessUtil.StartProcess(Executable);

    public void Stop()
    {
        if (IsRunning)
        {
            ProcessUtil.StopProcess(Executable);
        }
    }

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
            Games = EpicGameFactory.GetGames(this, cancellationToken);
        }
    }
    #endregion

    #region Private methods
    private static string? GetExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("com.epicgames.launcher");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.CurrentUser, @"Software\Epic Games\EOS", "ModSdkCommand");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!PathUtil.IsExecutable(executablePath))
        {
            executablePath = null;
        }

        return executablePath;
    }
    #endregion
}