using Gamelib.Core.Util;
using GameLib.Core;
using System.Drawing;

namespace GameLib.Plugin.Ubisoft.Model;

public class UbisoftGame : IGame
{
    #region Interface implementations
    public string Id { get; internal set; } = string.Empty;
    public Guid LauncherId { get; internal set; } = Guid.Empty;
    public string Name { get; internal set; } = string.Empty;
    public string InstallDir { get; internal set; } = string.Empty;
    public string Executable { get; internal set; } = string.Empty;
    public Icon? ExecutableIcon => PathUtil.GetFileIcon(Executable);
    public string WorkingDir { get; internal set; } = string.Empty;
    public string LaunchString { get; internal set; } = string.Empty;
    public DateTime InstallDate { get; internal set; } = DateTime.MinValue;
    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);
    #endregion

    public string Language { get; internal set; } = string.Empty;
    public string HelpUrl { get; internal set; } = string.Empty;
    public string FacebookUrl { get; internal set; } = string.Empty;
    public string HomepageUrl { get; internal set; } = string.Empty;
    public string ForumUrl { get; internal set; } = string.Empty;
}