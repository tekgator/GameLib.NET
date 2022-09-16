namespace GameLib.Plugin.Steam.Model;

internal class DeserializedSteamGame
{
    public uint AppId { get; set; }
    public uint Universe { get; set; }
    public string? Name { get; set; }
    public string? InstallDir { get; set; }
    public string? LaunchString { get; set; }
    public long LastUpdated { get; set; }
    public long SizeOnDisk { get; set; }
    public long LastOwner { get; set; }
    public bool AutoUpdateBehavior { get; set; }
    public bool AllowOtherDownloadsWhileRunning { get; set; }
    public long ScheduledAutoUpdate { get; set; }
}
