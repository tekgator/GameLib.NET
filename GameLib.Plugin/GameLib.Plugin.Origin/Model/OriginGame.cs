using Gamelib.Core.Util;
using GameLib.Core;

namespace GameLib.Plugin.Origin.Model;

public class OriginGame : IGame
{
    #region Interface implementations
    public string Id { get; internal set; } = string.Empty;
    public string Name { get; internal set; } = string.Empty;
    public string InstallDir { get; internal set; } = string.Empty;
    public string ExecutablePath { get; internal set; } = string.Empty;
    public string Executable { get; internal set; } = string.Empty;
    public string WorkingDir { get; internal set; } = string.Empty;
    public string LaunchString { get; internal set; } = string.Empty;
    public DateTime InstallDate { get; internal set; } = DateTime.MinValue;
    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);
    #endregion

    public string Locale { get; set; } = string.Empty;
    public long TotalBytes { get; set; } = 0;
}
