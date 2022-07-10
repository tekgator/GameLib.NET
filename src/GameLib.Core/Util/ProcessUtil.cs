using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Gamelib.Core.Util;

public static class ProcessUtil
{
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);
    private const uint WM_QUERYENDSESSION = 0x11;
    private const uint ENDSESSION_CLOSEAPP = 0x1;
    private const uint WM_ENDSESSION = 0x16;

    /// <summary>
    /// Check whether a executable is running
    /// </summary>
    public static bool IsProcessRunning(string? executable) =>
        !string.IsNullOrEmpty(executable) &&
        Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executable)).Any(p => !p.HasExited);

    /// <summary>
    /// Starts a process
    /// </summary>
    /// <returns><see langword="true"/> if process started successfully</returns>
    public static bool StartProcess(string? executable)
    {
        if (string.IsNullOrEmpty(executable))
            return false;

        return Process.Start(executable) is not null;
    }

    /// <summary>
    /// Forcefully stops a process
    /// </summary>
    public static void StopProcess(string? executable)
    {
        if (string.IsNullOrEmpty(executable))
            return;

        foreach (var runningProcess in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(executable)).Where(p => !p.HasExited))
        {
            var res = SendMessage(runningProcess.MainWindowHandle, WM_QUERYENDSESSION, 0, new IntPtr(ENDSESSION_CLOSEAPP));
            if ((uint)res != 0)
            {
                res = SendMessage(runningProcess.MainWindowHandle, WM_ENDSESSION, 0, new IntPtr(ENDSESSION_CLOSEAPP));
                if ((uint)res != 0)
                    continue;
            }

            if (!runningProcess.HasExited)
                runningProcess.Kill(true);
        }
    }
}
