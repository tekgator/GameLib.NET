using Gamelib.Core.Util;
using GameLib.Core;
using System.Drawing;

namespace GameLib.Plugin.RiotGames.Model;

public class Game : IGame
{
    public string Id { get; internal set; } = string.Empty;
    public Guid LauncherId { get; internal set; } = Guid.Empty;
    public string Name { get; internal set; } = string.Empty;
    public string InstallDir { get; internal set; } = string.Empty;
    public string Executable { get; internal set; } = string.Empty;
    public Icon? ExecutableIcon => PathUtil.GetFileIcon(Executable);
    public IEnumerable<string> Executables { get; internal set; } = Enumerable.Empty<string>();
    public string WorkingDir { get; internal set; } = string.Empty;
    public string LaunchString { get; internal set; } = string.Empty;
    public DateTime InstallDate { get; internal set; } = DateTime.MinValue;
    public bool IsRunning => ProcessUtil.IsProcessRunning(Executable);
}