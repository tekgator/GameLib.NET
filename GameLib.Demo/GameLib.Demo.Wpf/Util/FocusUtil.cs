using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GameLib.Demo.Wpf.Util;

public static class FocusUtil
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr WindowHandle);

    private const int SW_RESTORE = 9;

    public static void FocusProcess(string procName)
    {
        Process[] objProcesses = Process.GetProcessesByName(procName);
        if (objProcesses.Length > 0)
        {
            IntPtr hWnd = objProcesses[0].MainWindowHandle;
            ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
            SetForegroundWindow(hWnd);
        }
    }
}
