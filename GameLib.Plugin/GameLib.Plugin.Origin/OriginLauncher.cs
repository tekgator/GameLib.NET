﻿using Gamelib.Core.Util;
using GameLib.Core;
using Microsoft.Win32;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.InteropServices;

namespace GameLib.Plugin.Origin;

[Guid("54C9D299-107E-4990-894D-9DB402F81CA3")]
[Export(typeof(ILauncher))]
public class OriginLauncher : ILauncher
{
    [ImportingConstructor]
    public OriginLauncher(LauncherOptions? launcherOptions)
    {
        LauncherOptions = launcherOptions ?? new LauncherOptions();
    }

    #region Interface implementations
    public LauncherOptions LauncherOptions { get; }

    public Guid Id => GetType().GUID;

    public string Name => "Origin";

    public Image Logo => Properties.Resources.Logo256px;

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
            Games = OriginGameFactory.GetGames(this, cancellationToken);
        }
    }
    #endregion

    #region Private methods
    private static string? GetExecutable()
    {
        string? executablePath = null;

        executablePath ??= RegistryUtil.GetShellCommand("origin");
        executablePath ??= RegistryUtil.GetShellCommand("origin2");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Origin", "ClientPath");
        executablePath ??= RegistryUtil.GetValue(RegistryHive.LocalMachine, @"SOFTWARE\Origin", "OriginPath");

        executablePath = PathUtil.Sanitize(executablePath);

        if (!PathUtil.IsExecutable(executablePath))
        {
            executablePath = null;
        }

        return executablePath;
    }
    #endregion
}