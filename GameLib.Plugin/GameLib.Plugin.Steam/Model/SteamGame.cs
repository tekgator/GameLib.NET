using Gamelib.Core.Util;
using GameLib.Core;
using Microsoft.Win32;

namespace GameLib.Plugin.Steam.Model;

public class SteamGame : IGame
{
    #region Interface implementations
    public string Id { get; internal set; } = string.Empty;
    public Guid LauncherId { get; internal set; } = Guid.Empty;
    public string Name { get; internal set; } = string.Empty;
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
                return Convert.ToBoolean(RegistryUtil.GetValue(RegistryHive.CurrentUser, $@"Software\Valve\Steam\Apps\{Id}", "Running"));

            return ProcessUtil.IsProcessRunning(Executable);
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
        Convert.ToBoolean(RegistryUtil.GetValue(RegistryHive.CurrentUser, $@"Software\Valve\Steam\Apps\{Id}", "Updating"));

    // From catalog data
    public string Developer { get; internal set; } = string.Empty;
    public string DeveloperUrl { get; internal set; } = string.Empty;
    public string Publisher { get; internal set; } = string.Empty;
    public string Homepage { get; internal set; } = string.Empty;
    public string GameManualUrl { get; internal set; } = string.Empty;
}
