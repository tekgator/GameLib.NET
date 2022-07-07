using Gamelib.Util;
using Microsoft.Win32;
using System.Diagnostics;

namespace GameLib.Plugin.Steam.Model;

public class SteamGame : IGame
{
    #region Interface implementations
    public string GameId { get; internal set; } = string.Empty;
    public string GameName { get; internal set; } = string.Empty;
    public string InstallDir { get; internal set; } = string.Empty;
    public string ExecutablePath { get; internal set; } = string.Empty;
    public string Executable { get; internal set; } = string.Empty;
    public string WorkingDir { get; internal set; } = string.Empty;
    public string LaunchString { get; internal set; } = string.Empty;
    public DateTime InstallDate { get; internal set; } = DateTime.MinValue;
    public bool IsRunning
    {
        get
        {
            if (string.IsNullOrEmpty(Executable))
                return Convert.ToBoolean(RegistryUtil.GetValue(RegistryHive.CurrentUser, $@"Software\Valve\Steam\Apps\{GameId}", "Running"));

            return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Executable)).Where(p => !p.HasExited).Any();
        }
    }
    #endregion

    public SteamUniverse Universe { get; internal set; } = SteamUniverse.Invalid;
    public DateTime LastUpdated { get; internal set; } = DateTime.MinValue;
    public long SizeOnDisk { get; internal set; } = 0;
    public long LastOwner { get; internal set; } = 0;
    public bool AutoUpdateBehavior { get; internal set; } = false;
    public bool AllowOtherDownloadsWhileRunning { get; internal set; } = false;
    public DateTime ScheduledAutoUpdate { get; internal set; } = DateTime.MinValue;
    public bool IsUpdating =>
        Convert.ToBoolean(RegistryUtil.GetValue(RegistryHive.CurrentUser, $@"Software\Valve\Steam\Apps\{GameId}", "Updating"));
}
