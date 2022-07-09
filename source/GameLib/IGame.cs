namespace GameLib;

public interface IGame
{
    public string GameId { get; }
    public string GameName { get; }
    public string InstallDir { get; }
    public string ExecutablePath { get; }
    public string Executable { get; }
    public string WorkingDir { get; }
    public string LaunchString { get; }
    public DateTime InstallDate { get; }
    public bool IsRunning { get; }
}
