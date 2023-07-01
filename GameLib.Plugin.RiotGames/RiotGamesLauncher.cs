using GameLib.Core;
using System.Runtime.InteropServices;
using System.ComponentModel.Composition;
using System.Drawing;
using Gamelib.Core.Util;
using Microsoft.Win32;
using System.IO;

namespace GameLib.Plugin.RiotGames;

[Guid("54C9D299-555E-4990-894D-9DB402F81CA3")]
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
            Games = RiotGamesFactory.GetGames(this, cancellationToken).GetAwaiter().GetResult();
        }
    }

    private static string? GetExecutable()
    {
        RegistryKey uninstallKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
        var programs = uninstallKey.GetSubKeyNames();

        foreach (var program in programs)
        {
            RegistryKey subkey = uninstallKey.OpenSubKey(program);
            if (string.Equals("Riot Games, Inc", subkey.GetValue("Publisher", string.Empty).ToString(), StringComparison.CurrentCulture))
            {
                var k = subkey.GetValue("UninstallString").ToString().
                    Replace("\\RiotClientServices.exe\" --uninstall-product=bacon --uninstall-patchline=live", "")
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