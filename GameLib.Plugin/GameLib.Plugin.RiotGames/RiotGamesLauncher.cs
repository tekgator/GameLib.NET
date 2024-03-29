﻿using GameLib.Core;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.Drawing;
using Gamelib.Core.Util;
using Microsoft.Win32;

namespace GameLib.Plugin.RiotGames;

[Guid("938071a9-925e-4b1d-ae17-61a05ef5ec28")]
[Export(typeof(ILauncher))]
public class RiotGamesLauncher : ILauncher
{
    [ImportingConstructor]
    public RiotGamesLauncher(LauncherOptions? launcherOptions)
    {
        LauncherOptions = launcherOptions ?? new LauncherOptions();
    }

    public LauncherOptions LauncherOptions { get; }

    public Guid Id => GetType().GUID;

    public string Name => "Riot Games";

    public Image Logo => Properties.Resources.RiotGames;

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
            Games = RiotGamesFactory.GetGames(this, cancellationToken).GetAwaiter().GetResult();
        }
    }

    private static string? GetExecutable()
    {
        var uninstallKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");

        if (uninstallKey == null)
            return string.Empty;
        
        var programs = uninstallKey.GetSubKeyNames();
        
        foreach (var program in programs)
        {
            var subkey = uninstallKey.OpenSubKey(program);
            if (subkey == null) continue;
            
            if (string.Equals("Riot Games, Inc", subkey.GetValue("Publisher", string.Empty)?.ToString(), StringComparison.CurrentCulture))
            {
                var k = subkey.GetValue("UninstallString")?.ToString()?
                    .Replace("\\RiotClientServices.exe\" --uninstall-product=bacon --uninstall-patchline=live", "")
                    .Replace("\\RiotClientServices.exe\" --uninstall-product=valorant --uninstall-patchline=live", "")
                    .Replace("\\RiotClientServices.exe\" --uninstall-product=league_of_legends --uninstall-patchline=live", "")
                    .Replace("\\RiotClientServices.exe\" --uninstall-product=league_of_legends_game --uninstall-patchline=live", "")
                    .Replace("\"", "");

                return string.IsNullOrEmpty(k) ? string.Empty : Path.Combine(k, "RiotClientServices.exe");
            }
        }

        return string.Empty;
    }
}