using System.Diagnostics;

namespace GameLib.Util;

public static class ProcessUtil
{
    /// <summary>
    /// Check whether a executable is running
    /// </summary>
    public static bool IsProcessRunning(string? executable) =>
        !string.IsNullOrEmpty(executable) &&
        Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executable)).Any(p => !p.HasExited);

    /// <summary>
    /// Forcefully stops a process
    /// </summary>
    public static void StopProcess(string? executable)
    {
        if (string.IsNullOrEmpty(executable))
            return;

        foreach (var runningProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executable)).Where(p => !p.HasExited))
        {
            runningProcess.Kill(true);
        }
    }
}
